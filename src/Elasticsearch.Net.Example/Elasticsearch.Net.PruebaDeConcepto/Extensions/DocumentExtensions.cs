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
            destination.ids_procesos = destination.ids_procesos ?? new List<string>();
            changes.ids_procesos = changes.ids_procesos ?? new List<string>();
            //Mezclando ids_procesos
            destination.ids_procesos = destination.ids_procesos.Union(changes.ids_procesos).ToList();

            //Remplazo de administrados
            destination.administrados = (changes.administrados != null && changes.administrados.Any()) ? changes.administrados : destination.administrados;

            destination.solici_numero = changes.solici_numero ?? destination.solici_numero;
            destination.solici_id_estado = changes.solici_id_estado ?? destination.solici_id_estado;
            destination.solici_fecha_registro = changes.solici_fecha_registro ?? destination.solici_fecha_registro;

        }
    }
}
