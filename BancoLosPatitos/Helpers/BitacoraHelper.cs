using BancoLosPatitos.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.Helpers
{
    public class BitacoraHelper
    {
        public static void RegistrarEvento(PatitosContext db, string tabla, string tipoEvento, object datosAnteriores, object datosPosteriores = null, string descripcion = "", string stackTrace = "")
        {
            var bitacora = new Bitacora
            {
                TablaDeEvento = tabla,
                TipoDeEvento = tipoEvento,
                FechaDeEvento = DateTime.Now,
                Descripcion = string.IsNullOrWhiteSpace(descripcion) ? $"Evento {tipoEvento} en tabla {tabla}" : descripcion,
                StackTrace = string.IsNullOrWhiteSpace(stackTrace) ? "-" : stackTrace,
                DatosAnteriores = datosAnteriores != null ? JsonConvert.SerializeObject(datosAnteriores) : null,
                DatosPosteriores = datosPosteriores != null ? JsonConvert.SerializeObject(datosPosteriores) : null
            };

            db.Bitacoras.Add(bitacora);
            db.SaveChanges();
        }

        public static void RegistrarError(PatitosContext db, string tabla, Exception ex)
        {
            RegistrarEvento(db,tabla,"Error",null,null,ex.Message,ex.StackTrace);
        }
    }
}