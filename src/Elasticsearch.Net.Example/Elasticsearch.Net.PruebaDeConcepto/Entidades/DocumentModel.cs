using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    public class DocumentModel
    {
        public DocumentModel()
        {
            ids_procesos = new List<string>();
            administrados = new List<AdministradoModel>();
        }

        //Contexto
        public string id_proceso_base { get; set; }
        public string id_flujo { get; set; }
        public List<string> ids_procesos { get; set; }
        public List<AdministradoModel> administrados { get; set; }

        //Solicitud
        public string solici_numero { get; set; }
        public string solici_id_estado { get; set; }
        public DateTime? solici_fecha_registro { get; set; }
    }
}
