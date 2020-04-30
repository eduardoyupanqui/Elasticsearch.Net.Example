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
using System.Text;
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
                    .DefaultMappingFor<DocumentModel>(m => m
                        .IndexName(_elasticConfig.METADATA_INDEX)
                        .IdProperty(d => d.IdProcesoBase));

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
            await CrearIndexSiNoExiste<DocumentModel>(_elasticClient);

            //1 Registrar documento en el indice
            foreach (var request in DummyData.ObtenerSolicitudesDummy3())
            {
                Console.WriteLine($"Agregando documento: {request.IdProcesoBase}");
                await RegistrarDocumentModel(_elasticClient, request);
            }

            //await BulkOperatorsBasic(_elasticClient, DummyData.ObtenerSolicitudesDummy4().First(), DummyData.ObtenerSolicitudesDummy3().First());
            await BulkOperatorsSmart(_elasticClient, DummyData.ObtenerSolicitudesDummy4().First(), DummyData.ObtenerSolicitudesDummy3().First());

            //2 Buscar documento en el indice

            var requestBusqueda = DummyData.GetRequest();

            var response = await BuscarMetadatos(_elasticClient, requestBusqueda);

            foreach (var document in response)
            {
                Console.WriteLine(BeautyDocument(document));
            }

            Console.WriteLine("Bye World Elasticsearch.Net!");
            Console.ReadKey();
        }

        private static string BeautyDocument(BuscarMetadatosResponse document)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("********************************************************");
            stringBuilder.AppendLine($"Proceso Id: {document.id_proceso_base } Num Sol: {document.solici_numero}");
            stringBuilder.AppendLine($"Fecha: {document.solici_fecha_registro }");
            foreach (var administrado in document.administrados)
            {
                stringBuilder.AppendLine($"Administrado: {administrado.tipo_documento} {administrado.numero_documento} Num Sol: {document.solici_numero}");
                stringBuilder.AppendLine($"Descripcion: {administrado.descripcion}");
            }

            stringBuilder.AppendLine("********************************************************");
            return stringBuilder.ToString();

        }

        private static async Task CrearIndexSiNoExiste<T>(IElasticClient elasticClient)
        {
            var existsResponse = await elasticClient.Indices.ExistsAsync(Indices.Index<T>());
            if (!existsResponse.Exists)
            {
                await elasticClient.Indices.CreateAsync(Indices.Index<T>(), c => c
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
            //Opcion 1
            //var filters = new List<Func<QueryContainerDescriptor<DocumentModel>, QueryContainer>>();
            //filters.Add(fq => fq.Match(w => w.Field(f => f.IdProcesoBase).Query(request.IdProcesoBase)));
            //ISearchResponse<DocumentModel> searchResponse = await _elasticClient.SearchAsync<DocumentModel>(x =>
            //    x.Query(q => q
            //        .Bool(bq => bq
            //            .Filter(filters))
            //        )
            //    );
            //Opcion 2
            var getResponse = await _elasticClient.GetAsync<DocumentModel>(request.IdProcesoBase);
            //Opcion 3
            //var existsResponse = await _elasticClient.DocumentExistsAsync<DocumentModel>(request.IdProcesoBase);

            //2) si ya existe, se obtiene
            if (!getResponse.Found)
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
                DocumentModel document = getResponse.Source;
                document.MergeFrom(request);

                var updateResponse = await _elasticClient.UpdateAsync<DocumentModel>(DocumentPath<DocumentModel>.Id(document),
                    u => u
                        .DocAsUpsert(true)
                        .Doc(document)
                    );
                if (!updateResponse.IsValid)
                {
                    Console.WriteLine("Error al actualizar bloque");
                    Console.WriteLine(updateResponse.DebugInformation);
                }
            }
        }
        //https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-bulk.html
        public static async Task BulkOperatorsBasic(IElasticClient _elasticClient, DocumentModel documentToAdd, DocumentModel documentToUpdate)
        {
            //Operator para agregar Documentos
            var bulkCreateOperator = new BulkCreateOperation<DocumentModel>(documentToAdd);
            //Operator para actualizar Documentos
            var documentPartialUpdateModel = new DocumentPartialUpdateModel { SoliciIdEstado = "Pendiente"};
            var bulkUpdateOperator = new BulkUpdateOperation<DocumentModel, DocumentPartialUpdateModel>(documentToUpdate, documentPartialUpdateModel);
            var bulkResponse = await _elasticClient.BulkAsync(b => b.AddOperation(bulkCreateOperator).AddOperation(bulkUpdateOperator));
            if (!bulkResponse.IsValid)
            {
                //var responseBeauty = bulkResponse.Items
                Console.WriteLine("Error al agregar/actualizar bloques");
                Console.WriteLine(bulkResponse.DebugInformation);
            }
        }

        public static async Task BulkOperatorsSmart(IElasticClient _elasticClient, DocumentModel documentToAdd, DocumentModel documentToUpdate)
        {
            //Operator para agregar/actualizar Documentos (UPSERT)
            var bulkRequest = new BulkRequest() { Operations = new BulkOperationsCollection<IBulkOperation>() };
            bulkRequest.Operations.Add(new BulkUpdateOperation<DocumentModel, DocumentModel>(documentToAdd, documentToAdd, useIdFromAsUpsert: true));
            bulkRequest.Operations.Add(new BulkUpdateOperation<DocumentModel, DocumentModel>(documentToUpdate, documentToUpdate, useIdFromAsUpsert: true));
            var bulkResponse = await _elasticClient.BulkAsync(bulkRequest);
            if (!bulkResponse.IsValid)
            {
                //var responseBeauty = bulkResponse.Items
                Console.WriteLine("Error al agregar/actualizar bloques");
                Console.WriteLine(bulkResponse.DebugInformation);
            }
        }

        public static async Task<IEnumerable<BuscarMetadatosResponse>> BuscarMetadatos(IElasticClient _elasticClient, BuscarMetadatosRequest request)
        {

            //Acumulador de filtros
            var filters = new List<Func<QueryContainerDescriptor<DocumentModel>, QueryContainer>>();

            //0)Filtrando por id_flujo
            if (!string.IsNullOrEmpty(request.id_flujo))
            {
                filters.Add(fq => fq.Terms(w => w.Field(f => f.IdFlujo).Terms<string>(request.id_flujo)));
            }

            //1)Filtrando por ids_procesos_base
            if (request.ids_procesos_base != null && request.ids_procesos_base.Count > 0)
            {
                filters.Add(fq => fq.Terms(w => w.Field(f => f.IdProcesoBase).Terms<string>(request.ids_procesos_base)));
            }

            //2)Filtrando por ids_procesos
            if (request.ids_procesos != null && request.ids_procesos.Count > 0)
            {
                filters.Add(fq => fq.Terms(w => w.Field(f => f.IdsProcesos).Terms<string>(request.ids_procesos)));
            }
            //3)Filtrando por administrado
            if (request.administrado != null)
            {

                //Search Nested Query
                //GET metadata_index/ _search
                //{
                //  "query": {
                //        "nested" : {
                //            "path" : "administrados",
                //            "query" : {
                //                "bool" : {
                //                    "must" : [
                //                      { "term" : {"administrados.id_administrado" : "f1baf60d-74f1-46ce-89ee-d16f68772807"}}
                //                    ]
                //                }
                //            },
                //            "score_mode" : "avg"
                //        }
                //  }
                //}

                //#{ "term" : {"administrados.numero_documento" : "20138149022"} }
                //#{ "term" : {"administrados.tipo_documento" : "RUC"} }
                //#{ "term" : {"administrados.id_administrado" : "ace82812-ccdd-4ff0-8021-b21534387aa3"} }
                //#{ "match" : {"administrados.descripcion" : "Universidad"} } //Agregar el wildcard con should/must
                //#{ "wildcard" : {"administrados.descripcion" : "nacional*"} }

                var mustes = new List<Func<QueryContainerDescriptor<DocumentModel>, QueryContainer>>();

                //1) Búsqueda por descripción de administrado (nombre o razón social) (wildcard)
                if (!string.IsNullOrWhiteSpace(request.administrado.descripcion))
                {
                    //#region Preproceso de wildcards
                    var query = request.administrado.descripcion.ToLower()
                                .Replace("*", string.Empty)
                                .Replace("?", string.Empty);
                    //#endregion
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query += "*";
                        //filters.Add(fq => fq.Wildcard(x => new WildcardQueryDescriptor<BloqueMetadatos>().Field("administrados.descripcion").Value(query)));
                        //filters.Add(fq => fq.Nested(c => c
                        //    .Path(p => p.Administrados)
                        //    .Query(q => q
                        //        .Wildcard(x => new WildcardQueryDescriptor<DocumentModel>().Field("administrados.descripcion").Value(query)))));

                        //Refactor
                        //mustes.Add(qn => qn.Match(nq => nq.Field("administrados.descripcion").Query(request.administrado.descripcion)));
                        mustes.Add(qn => qn.Wildcard(x => new WildcardQueryDescriptor<DocumentModel>().Field(x => x.Administrados.First().Descripcion).Value(query)));
                    }
                }


                //2) Búsqueda por tipo de documento
                if (!string.IsNullOrWhiteSpace(request.administrado.tipo_documento))
                {
                    mustes.Add(qn => qn.Terms(nq => nq.Field(x => x.Administrados.First().TipoDocumento).Terms(request.administrado.tipo_documento)));
                }

                //2) Búsqueda por número de documento
                if (!string.IsNullOrWhiteSpace(request.administrado.numero_documento))
                {
                    ////filters.Add(fq => fq.Terms(w => w.Field("administrados.numero_documento").Terms<string>(request.administrado.numero_documento)));
                    //filters.Add(fq => fq.Nested(c => c
                    //    .Path(p => p.Administrados)
                    //    .Query(q => q
                    //        .Match(nq => nq.Field("administrados.numero_documento").Query(request.administrado.numero_documento)))));
                    
                    //Refactor
                    mustes.Add(qn => qn.Terms(nq => nq.Field(x => x.Administrados.First().NumeroDocumento).Terms(request.administrado.numero_documento)));
                }

                //3) Búsqueda por id administrado
                if (!string.IsNullOrWhiteSpace(request.administrado.id_administrado))
                {
                    ////filters.Add(fq => fq.Match(w => w.Field("administrados.id_administrado").Query(request.administrado.id_administrado)));
                    //filters.Add(fq => fq.Nested(c => c
                    //    .Path(p => p.Administrados)
                    //    .Query(q => q
                    //        .Match(nq => nq.Field("administrados.id_administrado").Query(request.administrado.id_administrado)))));
                    
                    //Refactor
                    mustes.Add(qn => qn.Terms(nq => nq.Field(x => x.Administrados.First().IdAdministrado).Terms(request.administrado.id_administrado)));
                }

                if (mustes.Count > 0)
                {
                    filters.Add(fq => fq.Nested(c => c
                            .Path(p => p.Administrados)
                            .Query(q => q.Bool(b => b.Must(mustes))
                                )));
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

            //5) Filtrando por rango de fechas desde-hasta

            //GET metadata_index/_search
            //{
            //  "query": {
            //    "bool": {
            //      "filter": [
            //        {
            //          "range": {
            //            "solici_fecha_registro": {
            //              "gte": "2020-04-21T00:00:00.0000000-05:00",
            //              "lt": "2020-04-23T00:00:00.0000000-05:00"
            //            }
            //          }
            //        }
            //      ]
            //    }
            //  }
            //}
            {
                filters.Add(fq => fq
                            .DateRange(c => c
                            .Field(d => d.SoliciFechaRegistro)
                            .GreaterThanOrEquals(request.desde)
                            .LessThan(request.hasta)
                            )
                            );
            }

            //GET metadata_index/ _search
            //{
            //      "query": {
            //          "bool": {
            //              "must" : {
            //                 "term" : { "es_pendiente" : true }
            //              },
            //              "filter": [
            //                  {
            //                     ....
            //                  }
            //              ]
            //          }
            //      }
            //}
            ISearchResponse<DocumentModel> searchResponse = await _elasticClient.SearchAsync<DocumentModel>(x => x
                .Query(q => q.Bool(bq => bq.Must(qd => qd.Term(f => f.SoliciIdEstado, "Pendiente")).Filter(filters)))
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
