using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace Elasticsearch.Net.PruebaDeConcepto.Entidades
{
    //[ElasticsearchType(RelationName = "_doc")]
    public partial class DocumentModel
    {
        public DocumentModel()
        {
            IdsProcesos = new List<string>();
        }

        [Keyword(Name = "id_flujo")]
        public string IdFlujo { get; set; } //ES: keyword

        [Keyword(Name = "id_proceso_base")]
        public string IdProcesoBase { get; set; }
        [PropertyName("ids_procesos")]
        public List<string> IdsProcesos { get; set; } //ES: keyword

    }

    /// <summary>
    /// Campos relacionados a entidad "Solicitud" (prefijo "solici")
    /// </summary>
    public partial class DocumentModel
    {
        [Keyword(Name = "solici_numero")]
        public string SoliciNumero { get; set; }

        [Keyword(Name = "solici_id_estado")]
        public string SoliciIdEstado { get; set; }

        [Date(Name = "solici_fecha_registro", Format = "date_optional_time")]
        public DateTime? SoliciFechaRegistro { get; set; }
    }

    public partial class DocumentModel
    {
        [Nested]
        [PropertyName("administrados")]
        public List<Administrado> Administrados { get; set; }
    }

    public class Administrado
    {
        [Keyword(Name = "id_administrado")]
        public string IdAdministrado { get; set; }

        [Keyword(Name = "tipo_documento")]
        public string TipoDocumento { get; set; }

        [Keyword(Name = "numero_documento")]
        public string NumeroDocumento { get; set; }

        /// <summary>
        /// Razon social o nombres y apellidos
        /// </summary>
        [Text(Name = "descripcion")]
        public string Descripcion { get; set; }
    }
}
