using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.Models
{
    public class Sinpe
    {
        [Key]
        public int IdSinpe { get; set; }

        [Required]
        [StringLength(10)]
        public string TelefonoOrigen { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreOrigen { get; set; }

        [Required]
        [StringLength(10)]
        public string TelefonoDestinatario { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreDestinatario { get; set; }

        [Required]
        [Range(0.01, 100000000000000000)]
        public decimal Monto { get; set; }

        [Required]
        public DateTime FechaDeRegistro { get; set; }

        [StringLength(50)]
        public string Descripcion { get; set; }

        public byte Estado { get; set; }

    }
}