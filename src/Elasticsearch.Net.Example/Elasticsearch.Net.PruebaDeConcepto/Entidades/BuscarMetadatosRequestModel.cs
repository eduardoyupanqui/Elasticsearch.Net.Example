using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    public class BuscarMetadatosRequest
    {
        public BuscarMetadatosRequest()
        {
            ids_procesos_base = new List<string>();
            ids_procesos = new List<string>();
            administrado = new AdministradoRequest();
        }
        public List<string> ids_procesos_base { get; set; }
        public List<string> ids_procesos { get; set; }
        public AdministradoRequest administrado { get; set; }

        /// <summary>
        /// Número de solicitud (valor exacto). Ejemplo "00038007-2018"
        /// </summary>
        public string solici_numero { get; set; }
    }

    public class AdministradoRequest
    {
        public string id_administrado { get; set; }
        public string tipo_documento { get; set; }
        public string numero_documento { get; set; }
        public string descripcion { get; set; }
    }
}
