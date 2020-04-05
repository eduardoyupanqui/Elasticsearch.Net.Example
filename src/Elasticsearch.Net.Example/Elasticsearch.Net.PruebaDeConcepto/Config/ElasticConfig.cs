using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Config
{
    public class ElasticConfig
    {
        public string ELASTIC_SERVER_URL { get; set; }
        public string ELASTIC_USER { get; set; }
        public string ELASTIC_PASS { get; set; }
        public string METADATA_INDEX { get; set; }
    }
}
