using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace BancoLosPatitos.Models
{
    public class Caja
    {
        [Key]
        public int IdCaja { get; set; }

        [Required]
        public int IdComercio { get; set; }

        [ForeignKey("IdComercio")]
        public Comercio Comercio { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(150)]
        public string Descripcion { get; set; }

        [Required]
        [StringLength(10)]
        public string TelefonoSINPE { get; set; }

        [Required]
        public DateTime FechaDeRegistro { get; set; }

        public DateTime FechaDeModificacion { get; set; }

        public byte Estado { get; set; }

    }
}