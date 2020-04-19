using Elasticsearch.Net.PruebaDeConcepto.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.Net.PruebaDeConcepto
{
    public class DummyData
    {
        public static IEnumerable<DocumentModel> ObtenerSolicitudesDummy1()
        {

            string id_documento_ruc = "15002";
            string id_estado_solicitud = "1";
            string base_numeracion = "2020-";
            DateTime fechaBase = DateTime.Now;

            var lstData = new List<dynamic>
            {
                new { id_proc="5bb3dd7d05617b595c76f15e", cod_entidad ="0002",ruc="20143660754", desc_ent = "Universidad Nacional del Centro del Perú"  },
                new { id_proc="5bad115c72e8d14d2cfcf1c4", cod_entidad ="0003",ruc="20172474501", desc_ent = "Universidad Nacional de San Antonio Abad del Cusco"  },
                new { id_proc="5b3e7c1e98f0db4df0dcf77b", cod_entidad ="0007",ruc="20147897406", desc_ent = "Universidad Nacional Agraria La Molina"  },
                new { id_proc="5bb3de4105617b595c76f165", cod_entidad ="0018",ruc="20107798049", desc_ent = "Universidad de Lima"  },
                new { id_proc="5ba2bd4611245d45b8d38eb0", cod_entidad ="0027",ruc="20138705944", desc_ent = "Universidad Nacional del Callao"  },
                new { id_proc="5b8d6d649510ef53a0b87fb9", cod_entidad ="0029",ruc="20172627421", desc_ent = "Universidad de Piura"  },
            };

            for (int i = 0; i < lstData.Count; i++)
            {
                var currentData = lstData[i];

                yield return new DocumentModel()
                {
                    IdProcesoBase = currentData.id_proc,
                    IdsProcesos = new List<string> { currentData.id_proc },
                    Administrados = new List<Administrado> {
                        new Administrado() {
                            IdAdministrado=currentData.cod_entidad,
                            TipoDocumento=id_documento_ruc,
                            NumeroDocumento =currentData.ruc,
                            Descripcion = currentData.desc_ent
                        }
                    },
                    SoliciNumero = base_numeracion + (i + 1).ToString("D3"),
                    SoliciIdEstado = id_estado_solicitud,
                    SoliciFechaRegistro = fechaBase.AddDays(i),
                };

            }
        }

        public static IEnumerable<DocumentModel> ObtenerSolicitudesDummy2()
        {

            string id_documento_ruc = "RUC";
            string id_estado_solicitud = "1";
            string base_numeracion = "2020-";
            DateTime fechaBase = DateTime.Now;

            var lstData = new List<dynamic>
            {
                new {
                    id_administrado="b63b8edc-6c02-45f4-91ed-e8913883debd"
                ,tipo_documento="RUC"
                ,numero_documento="20143660754"
                , id_flujo="5ab17b784fff3b05cc33897d"
                , id_proceso="5c8ad0c43cc3c423103920a1"
                ,ruc="20143660754"
                , descripcion = "Universidad Nacional del Centro del Perú"  },
               new {
                    id_administrado="b63b8edc-6c02-45f4-91ed-e8913883debd"
                ,tipo_documento="RUC"
                ,numero_documento="10426314239"
                , id_flujo="5ab17b784fff3b05cc33897d"
                , id_proceso="5c89a2f22f5cc51a1c8c5493"
                ,ruc="10426314239"
                , descripcion = "Instituto las cumbres"  },
            };

            for (int i = 0; i < lstData.Count; i++)
            {
                var currentData = lstData[i];

                yield return new DocumentModel()
                {
                    IdProcesoBase = currentData.id_proceso,
                    IdsProcesos = new List<string> { currentData.id_proceso },
                    Administrados = new List<Administrado> {
                        new Administrado() {
                            IdAdministrado=currentData.id_administrado,
                            TipoDocumento=currentData.tipo_documento,
                            NumeroDocumento =currentData.numero_documento,
                            Descripcion = currentData.descripcion
                        }
                    },
                    //solici_numero = base_numeracion + (i + 1).ToString("D3"),
                    //solici_id_estado = id_estado_solicitud,
                    SoliciFechaRegistro = fechaBase.AddDays(i),
                };
            }
        }

        public static IEnumerable<DocumentModel> ObtenerSolicitudesDummy3()
        {

            DateTime fechaBase = DateTime.Now;

            var lstData = new List<dynamic>
            {
                new {
                     id_administrado="ace82812-ccdd-4ff0-8021-b21534387aa3"
                    ,tipo_documento="RUC"
                    ,numero_documento="20138149022"
                    ,descripcion = "Universidad Nacional del Centro del Perú"
                    ,solici_numero="0004-SOLPROME"
                    ,id_flujo="5ab17b784fff3b05cc33897d"
                    ,id_proceso="5cf5a5820ee55a451c9f4a68"
                      },
               new {
                     id_administrado="f1baf60d-74f1-46ce-89ee-d16f68772807"
                    ,tipo_documento="RUC"
                    ,numero_documento="20148309109"
                    ,descripcion = "Universidad Nacional del Santa"
                    ,solici_numero="0018-SOLPROME"
                    ,id_flujo="5ab17b784fff3b05cc33897d"
                    ,id_proceso="5ce5c939d2cb0c503c628cac"
                      },
                new {
                     id_administrado="f1baf60d-74f1-46ce-89ee-d16f68772807"
                    ,tipo_documento="RUC"
                    ,numero_documento="20148309109"
                    ,descripcion = "Universidad Nacional del Santa"
                    ,solici_numero="0002-SOLPROME"
                    ,id_flujo="5ab17b784fff3b05cc33897d"
                    ,id_proceso="5cdae37d939be24b9ce0e020"
                      },
                 new {
                     id_administrado="2400736e-f593-472e-aad4-142a49c37957"
                    ,tipo_documento="RUC"
                    ,numero_documento="20479748102"
                    ,descripcion = "Universidad Señor de Sipán"
                    ,solici_numero="0008-SOLPROME"
                    ,id_flujo="5ab17b784fff3b05cc33897d"
                    ,id_proceso="5cdb2a8429f74744d07b31c0"
                      },
            };

            for (int i = 0; i < lstData.Count; i++)
            {
                var currentData = lstData[i];

                yield return new DocumentModel()
                {
                    IdProcesoBase = currentData.id_proceso,
                    IdFlujo = currentData.id_flujo,
                    IdsProcesos = new List<string> { currentData.id_proceso },
                    Administrados = new List<Administrado> {
                        new Administrado() {
                            IdAdministrado=currentData.id_administrado,
                            TipoDocumento=currentData.tipo_documento,
                            NumeroDocumento =currentData.numero_documento,
                            Descripcion = currentData.descripcion
                        }
                    },
                    SoliciNumero = currentData.solici_numero,
                    //solici_id_estado = id_estado_solicitud,
                    SoliciFechaRegistro = fechaBase.AddDays(i),
                };


            }
        }

        public static BuscarMetadatosRequest GetRequest()
        {

            var buscarRequest = new BuscarMetadatosRequest()
            {
                //1) Buscando por "id_administrado"
                administrado = new AdministradoRequest() { id_administrado = "f1baf60d-74f1-46ce-89ee-d16f68772807" }, //"Universidad Nacional del Santa"

                //2) Buscando por (parte de) "descripcion" de administrado
                //administrado = new AdministradoRequest() { descripcion = "priv" }, //"Universidad de San Martín de Porres"

                //3) Buscando por "numero_documento" de administrado
                //administrado = new AdministradoRequest() { numero_documento = "20479748102" }, //"Universidad Señor de Sipán"

                //4) Buscando por "id_proceso_base"
                //ids_procesos_base = new List<string>() { "5cdb2a8429f74744d07b31c0" }, //"0008-SOLPROME - Universidad Señor de Sipán"

                //5) Buscando por "ids_procesos"
                //ids_procesos = new List<string>() { "5cdae37d939be24b9ce0e020" }, //"0002-SOLPROME - Universidad Nacional del Santa"


                //6) Buscando por (parte de) "solici_numero"
                //solici_numero = "0002" // sin resultados, el campo "solici_numero" solo acepta búsquedas exactas

                //7) Buscando por "solici_numero"
                //solici_numero = "0008-SOLPROME" //"Universidad Señor de Sipán"
            };
            return buscarRequest;
        }
    }
}
