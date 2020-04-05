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

namespace Elasticsearch.Net.PruebaDeConcepto
{
    class Program
    {
        static void Main(string[] args)
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
                var settings = new ConnectionSettings(new Uri(_elasticConfig.ELASTIC_SERVER_URL))
                    //.DefaultIndex("defaultindex")
                    .DefaultMappingFor<DocumentModel>(m => m.IndexName("metadata_index"));

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

            //1 Registrar documento en el indice
            foreach (var request in DummyData.ObtenerSolicitudesDummy1())
            {
                RegistrarDocumentModel(_elasticClient, request);
            }

            //2 Buscar documento en el indice
        }



        public static void RegistrarDocumentModel(IElasticClient _elasticClient, DocumentModel request) 
        {
            
            //1) obtener documento que tenga el mismo "id_proceso_base" 
            var filters = new List<Func<QueryContainerDescriptor<DocumentModel>, QueryContainer>>();
            filters.Add(fq => fq.Match(w => w.Field("id_proceso_base").Query(request.id_proceso_base)));

            ISearchResponse<DocumentModel> searchResponse = _elasticClient.Search<DocumentModel>(x =>
                x.Query(q => q.Bool(bq => bq.Filter(filters))));

            //2) si ya existe, se obtiene
            

            if (searchResponse.Hits.Count == 0)
            {
                //Crear
                var insertResponse = _elasticClient.Index<DocumentModel>(new IndexRequest<DocumentModel>(request));
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

                var updateResponse = _elasticClient.Update<DocumentModel>(DocumentPath<DocumentModel>.Id(bloque_elasticId),
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

        public IEnumerable<BuscarMetadatosResponse> BuscarMetadatos(BuscarMetadatosRequest request) 
        { 
            return Enumerable.Empty<BuscarMetadatosResponse>();
        }

    }
}
