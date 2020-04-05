using Elasticsearch.Net.PruebaDeConcepto.Config;
using Elasticsearch.Net.PruebaDeConcepto.Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elasticsearch.Net.PruebaDeConcepto
{
    class Program
    {
        static void Main(string[] args)
        {

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var _elasticConfig = config.GetSection(nameof(ElasticConfig)).Get<ElasticConfig>();

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ElasticClient>((sp) => {
                var settings = new ConnectionSettings(new Uri(_elasticConfig.ELASTIC_SERVER_URL)).DefaultIndex(_elasticConfig.METADATA_INDEX);
                //settings.BasicAuthentication(_elasticConfig.ELASTIC_USER, _elasticConfig.ELASTIC_PASS);
#if DEBUG
                settings.DisableDirectStreaming(); //deshabilitado el streaming para capturar los request y response y examinarlos
#endif
                return new ElasticClient(settings);
            });

            Console.WriteLine("Hello World Elasticsearch.Net!");




        }

        public void RegistrarBloqueMetadatos(DocumentModel request) 
        {

        }

        public IEnumerable<BuscarMetadatosResponse> BuscarMetadatos(BuscarMetadatosRequest request) 
        { 
            return Enumerable.Empty<BuscarMetadatosResponse>();
        }

    }
}
