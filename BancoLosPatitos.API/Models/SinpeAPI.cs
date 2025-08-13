using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.API.Models
{
    public class SinpeAPI
    {
        public int IdSinpe { get; set; }
        public string TelefonoOrigen { get; set; }
        public string NombreOrigen { get; set; }
        public string TelefonoDestinatario { get; set; }
        public string NombreDestinatario { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; }
    }
}