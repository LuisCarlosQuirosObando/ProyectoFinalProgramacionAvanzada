using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.Models
{
    public class Reporte
    {
        [Key]
        public int IdReporte { get; set; }

        [Required]
        public int IdComercio { get; set; }

        [ForeignKey("IdComercio")]
        public Comercio Comercio { get; set; }

        [Required]
        public int CantidadDeCajas { get; set; }

        [Required]
        public decimal MontoTotalRecaudado { get; set; }

        [Required]
        public int CantidadDeSINPES { get; set; }

        [Required]
        public decimal MontoTotalComision { get; set; }

        [Required]
        public DateTime FechaDelReporte { get; set; }
    }
}