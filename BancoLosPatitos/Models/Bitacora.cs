using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.Models
{
    public class Bitacora
    {
        [Key]
        public int IdEvento { get; set; }

        [Required]
        [StringLength(20)]
        public string TablaDeEvento { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoDeEvento { get; set; }

        [Required]
        public DateTime FechaDeEvento { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public string StackTrace { get; set; }

        public string DatosAnteriores { get; set; }

        public string DatosPosteriores { get; set; }

    }
}