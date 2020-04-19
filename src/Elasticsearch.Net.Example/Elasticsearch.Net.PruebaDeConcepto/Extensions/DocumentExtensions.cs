using Elasticsearch.Net.PruebaDeConcepto.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto.Extensions
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Procedimiento de "obtención de cambios" desde un bloque de metadatos origen a uno destino.
        /// Solo se consideran como cambios los campos con valores distintos a null
        /// La lista de administrados se reemplaza
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="changes"></param>
        public static void MergeFrom(this DocumentModel destination, DocumentModel changes)
        {
            if (changes == null) return;
            //id_bloque no se combina
            //id_flujo no se combina

            //Mezcla ids_procesos:

            //Asegurando que las listas tengan valor
            destination.IdsProcesos = destination.IdsProcesos ?? new List<string>();
            changes.IdsProcesos = changes.IdsProcesos ?? new List<string>();
            //Mezclando ids_procesos
            destination.IdsProcesos = destination.IdsProcesos.Union(changes.IdsProcesos).ToList();

            //Remplazo de administrados
            destination.Administrados = (changes.Administrados != null && changes.Administrados.Any()) ? changes.Administrados : destination.Administrados;

            destination.SoliciNumero = changes.SoliciNumero ?? destination.SoliciNumero;
            destination.SoliciIdEstado = changes.SoliciIdEstado ?? destination.SoliciIdEstado;
            destination.SoliciFechaRegistro = changes.SoliciFechaRegistro ?? destination.SoliciFechaRegistro;

        }

        public static BuscarMetadatosResponse ToResponse(this DocumentModel entity)
        {
            var response = new BuscarMetadatosResponse();
            response.id_proceso_base = entity.IdProcesoBase;
            response.id_flujo = entity.IdFlujo;
            response.ids_procesos = entity.IdsProcesos;
            response.administrados = entity.Administrados.ToResponseList();

            response.solici_fecha_registro = entity.SoliciFechaRegistro;
            response.solici_id_estado = entity.SoliciIdEstado;
            response.solici_numero = entity.SoliciNumero;

            return response;
        }
        public static AdministradoResponse ToResponse(this Administrado entity)
        {
            return new AdministradoResponse()
            {
                id_administrado = entity.IdAdministrado,
                descripcion = entity.Descripcion,
                tipo_documento = entity.TipoDocumento,
                numero_documento = entity.NumeroDocumento
            };
        }

        public static List<AdministradoResponse> ToResponseList(this List<Administrado> entityList)
        {
            if (entityList == null) return null;
            var returnList = new List<AdministradoResponse>();
            foreach (var model in entityList)
            {
                returnList.Add(model.ToResponse());
            }

            return returnList;
        }
    }
}
