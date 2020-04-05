using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    [ElasticsearchType(Name = "_doc")]
    public partial class DocumentModel
    {
        public DocumentModel()
        {
            ids_procesos = new List<string>();
        }

        [Keyword(Name = "id_flujo")]
        public string id_flujo { get; set; } //ES: keyword

        [Keyword(Name = "id_proceso_base")]
        public string id_proceso_base { get; set; }

        public List<string> ids_procesos { get; set; } //ES: keyword

    }

    /// <summary>
    /// Campos relacionados a entidad "Solicitud" (prefijo "solici")
    /// </summary>
    public partial class DocumentModel
    {
        [Keyword(Name = "solici_numero")]
        public string solici_numero { get; set; }

        [Keyword(Name = "solici_id_estado")]
        public string solici_id_estado { get; set; }

        [Date(Name = "solici_fecha_registro", Format = "basic_date", IgnoreMalformed = true)]
        public DateTime? solici_fecha_registro { get; set; }
    }

    public partial class DocumentModel
    {
        [Nested]
        [PropertyName("administrados")]
        public List<Administrado> administrados { get; set; }
    }

    public class Administrado
    {
        [Keyword(Name = "id_administrado")]
        public string id_administrado { get; set; }

        [Keyword(Name = "tipo_documento")]
        public string tipo_documento { get; set; }

        [Keyword(Name = "numero_documento")]
        public string numero_documento { get; set; }

        /// <summary>
        /// Razon social o nombres y apellidos
        /// </summary>
        [Text(Name = "descripcion")]
        public string descripcion { get; set; }
    }
}
