using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    public class DocumentPartialUpdateModel
    {
        [Keyword(Name = "solici_id_estado")]
        public string SoliciIdEstado { get; set; }
    }
}
