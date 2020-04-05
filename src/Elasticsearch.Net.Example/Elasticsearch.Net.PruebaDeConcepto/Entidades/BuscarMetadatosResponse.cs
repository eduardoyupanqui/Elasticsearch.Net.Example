using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    public class BuscarMetadatosResponse
    {
        public BuscarMetadatosResponse()
        {
            ids_procesos = new List<string>();
            administrados = new List<AdministradoResponse>();
        }

        public string id_proceso_base { get; set; }
        public string id_flujo { get; set; } //ES: keyword
        public List<string> ids_procesos { get; set; } //ES: keyword
        public List<AdministradoResponse> administrados { get; set; }

        //Solicitud
        public string solici_numero { get; set; }
        public string solici_id_estado { get; set; }
        public DateTime? solici_fecha_registro { get; set; }
    }

    public class AdministradoResponse
    {
        public string id_administrado { get; set; }
        public string tipo_documento { get; set; }
        public string numero_documento { get; set; }
        public string descripcion { get; set; }
    }
}
