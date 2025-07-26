using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.Models
{
    public class ConfiguracionComercio
    {
        [Key]
        public int IdConfiguracion { get; set; }

        [Required]
        public int IdComercio { get; set; }

        [ForeignKey("IdComercio")]
        public Comercio Comercio { get; set; }

        [Required]
        public int TipoConfiguracion { get; set; }

        [Required]
        public int Comision { get; set; }

        [Required]
        public DateTime FechaDeRegistro { get; set; }

        public DateTime FechaDeModificacion { get; set; }

        [Required]
        public byte Estado { get; set; }
    }
}