using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    public class AdministradoModel
    {
        public string id_administrado { get; set; }
        public string tipo_documento { get; set; }
        public string numero_documento { get; set; }
        public string descripcion { get; set; }
    }
}
