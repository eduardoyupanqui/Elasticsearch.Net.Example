using Elasticsearch.Net.PruebaDeConcepto.Config;
using Elasticsearch.Net.PruebaDeConcepto.Entidades;
using Elasticsearch.Net.PruebaDeConcepto.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.PruebaDeConcepto
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Install https://www.elastic.co/es/downloads/past-releases/elasticsearch-6-5-4
            //>docker run -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:6.5.4
            //O
            //>docker-compose up
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var _elasticConfig = config.GetSection(nameof(ElasticConfig)).Get<ElasticConfig>();

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IElasticClient>((sp) => {
                var pool = new SingleNodeConnectionPool(new Uri(_elasticConfig.ELASTIC_SERVER_URL));
                var settings = new ConnectionSettings(pool)
                    //.DefaultIndex("defaultindex")
                    .DefaultMappingFor<DocumentModel>(m => m.IndexName(_elasticConfig.METADATA_INDEX));

                //settings.BasicAuthentication(_elasticConfig.ELASTIC_USER, _elasticConfig.ELASTIC_PASS);
#if DEBUG
                settings.DisableDirectStreaming(); //deshabilitado el streaming para capturar los request y response y examinarlos
#endif
                return new ElasticClient(settings);
            });

            Console.WriteLine("Hello World Elasticsearch.Net!");
            //Obteniendo el Client
            var serviceProvider = services.BuildServiceProvider();
            IElasticClient _elasticClient = serviceProvider.GetService<IElasticClient>();

            //0 Crear Index si no existe
            await CrearIndexSiNoExiste(_elasticClient, _elasticConfig.METADATA_INDEX);

            //1 Registrar documento en el indice
            foreach (var request in DummyData.ObtenerSolicitudesDummy3())
            {
                Console.WriteLine($"Agregando documento: {request.IdProcesoBase}");
                await RegistrarDocumentModel(_elasticClient, request);
            }

            //2 Buscar documento en el indice

            var requestBusqueda = DummyData.GetRequest();

            var response = await BuscarMetadatos(_elasticClient, requestBusqueda);

            foreach (var document in response)
            {
                Console.WriteLine($"Doc: {document.id_proceso_base } Num Sol: {document.solici_numero}");
            }

            Console.WriteLine("Bye World Elasticsearch.Net!");
            Console.ReadKey();
        }

        private static async Task CrearIndexSiNoExiste(IElasticClient elasticClient, string _currentIndexName)
        {
            var existsResponse = await elasticClient.Indices.ExistsAsync(Indices.Parse(_currentIndexName));
            if (!existsResponse.Exists)
            {
                await elasticClient.Indices.CreateAsync(_currentIndexName, c => c
                             .Settings(se => se
                                .NumberOfReplicas(0))
                             .Map<DocumentModel>(m => m
                                .AutoMap())
                             );
            }
        }

        public static async Task RegistrarDocumentModel(IElasticClient _elasticClient, DocumentModel request)
        {

            //1) obtener documento que tenga el mismo "id_proceso_base" 
            var filters = new List<Func<QueryContainerDescriptor<DocumentModel>, QueryContainer>>();
            filters.Add(fq => fq.Match(w => w.Field("id_proceso_base").Query(request.IdProcesoBase)));

            ISearchResponse<DocumentModel> searchResponse = await _elasticClient.SearchAsync<DocumentModel>(x =>
                x.Query(q => q.Bool(bq => bq.Filter(filters))));

            //2) si ya existe, se obtiene


            if (searchResponse.Hits.Count == 0)
            {
                //Crear
                var insertResponse = await _elasticClient.IndexAsync<DocumentModel>(new IndexRequest<DocumentModel>(request));
                if (!insertResponse.IsValid)
                {
                    Console.WriteLine("Error al registrar bloque");
                    Console.WriteLine(insertResponse.DebugInformation);
                }

            }
            else
            {
                //Modificar
                IHit<DocumentModel> hit = searchResponse.Hits.First();
                string bloque_elasticId = hit.Id;
                DocumentModel bloque = hit.Source;
                bloque.MergeFrom(request);

                var updateResponse = await _elasticClient.UpdateAsync<DocumentModel>(DocumentPath<DocumentModel>.Id(bloque_elasticId),
                    u => u
                        //.Index("metadata_index")
                        //.Type("_doc")
                        .DocAsUpsert(true)
                        .Doc(bloque)
                    );
                if (!updateResponse.IsValid)
                {
                    Console.WriteLine("Error al actualizar bloque");
                    Console.WriteLine(updateResponse.DebugInformation);
                }
            }
        }

        public static async Task<IEnumerable<BuscarMetadatosResponse>> BuscarMetadatos(IElasticClient _elasticClient, BuscarMetadatosRequest request)
        {

            //Acumulador de filtros
            var filters = new List<Func<QueryContainerDescriptor<DocumentModel>, QueryContainer>>();

            //1)Filtrando por ids_procesos_base
            if (request.ids_procesos_base != null && request.ids_procesos_base.Count > 0)
            {
                filters.Add(fq => fq.Terms(w => w.Field("id_proceso_base").Terms<string>(request.ids_procesos_base)));
            }

            //2)Filtrando por ids_procesos
            if (request.ids_procesos != null && request.ids_procesos.Count > 0)
            {
                filters.Add(fq => fq.Terms(w => w.Field("ids_procesos").Terms<string>(request.ids_procesos)));
            }
            //3)Filtrando por administrado
            if (request.administrado != null)
            {
                //1) Búsqueda por descripción de administrado (nombre o razón social) (wildcard)
                if (!string.IsNullOrWhiteSpace(request.administrado.descripcion))
                {
                    #region Preproceso de wildcards
                    var query = request.administrado.descripcion
                                .Replace("*", string.Empty)
                                .Replace("?", string.Empty);
                    #endregion
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query = query + "*";
                        //filters.Add(fq => fq.Wildcard(x => new WildcardQueryDescriptor<BloqueMetadatos>().Field("administrados.descripcion").Value(query)));
                        filters.Add(fq => fq.Nested(c => c
                            .Path(p => p.Administrados)
                            .Query(q => q
                                .Wildcard(x => new WildcardQueryDescriptor<DocumentModel>().Field("administrados.descripcion").Value(query)))));
                    }
                }

                //2) Búsqueda por número de documento
                if (!string.IsNullOrWhiteSpace(request.administrado.numero_documento))
                {
                    //filters.Add(fq => fq.Terms(w => w.Field("administrados.numero_documento").Terms<string>(request.administrado.numero_documento)));
                    filters.Add(fq => fq.Nested(c => c
                        .Path(p => p.Administrados)
                        .Query(q => q
                            .Match(nq => nq.Field("administrados.numero_documento").Query(request.administrado.numero_documento)))));
                }

                //3) Búsqueda por id administrado
                if (!string.IsNullOrWhiteSpace(request.administrado.id_administrado))
                {
                    //filters.Add(fq => fq.Match(w => w.Field("administrados.id_administrado").Query(request.administrado.id_administrado)));
                    filters.Add(fq => fq.Nested(c => c
                        .Path(p => p.Administrados)
                        .Query(q => q
                            .Match(nq => nq.Field("administrados.id_administrado").Query(request.administrado.id_administrado)))));
                }
            }

            //4) Filtrando por número de solicitud (solici_numero)
            if (!string.IsNullOrWhiteSpace(request.solici_numero))
            {
                filters.Add(fq => fq
                        .Match(w => w
                        .Field(d => d.SoliciNumero)
                        .Query(request.solici_numero)

                    )
                );
            }

            ISearchResponse<DocumentModel> searchResponse = await _elasticClient.SearchAsync<DocumentModel>(x => x
                .Query(q => q.Bool(bq => bq.Filter(filters)))
                .Sort(s => s.Ascending(SortSpecialField.DocumentIndexOrder))
                .TrackScores(false)
                //.Scroll("1m")
                .Skip(0)
                .Take(10)
                //.From(0)
                //.Size(1)
            );
            var results = new List<BuscarMetadatosResponse>();
            foreach (var hit in searchResponse.Hits)
            {
                results.Add(hit.Source.ToResponse());
            }

            return results;
            //return Enumerable.Empty<BuscarMetadatosResponse>();
        }

    }
}
