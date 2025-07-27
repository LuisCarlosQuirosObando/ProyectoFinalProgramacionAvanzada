using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BancoLosPatitos.Models
{
    public class Usuarios
    {

        [Key]
        public int IdUsuario { get; set; }

        [Required]
        public int IdComercio { get; set; }

        [ForeignKey("IdComercio")]
        public Comercio Comercio { get; set; }

        public Guid? IdNetUser { get; set; }

        [Required, StringLength(100)]
        public string Nombres { get; set; }

        [Required, StringLength(100)]
        public string PrimerApellido { get; set; }

        [Required, StringLength(100)]
        public string SegundoApellido { get; set; }

        [Required, StringLength(10)]
        public string Identificacion { get; set; }

        [Required, StringLength(200)]
        public string CorreoElectronico { get; set; }

        [Required]
        public DateTime FechaDeRegistro { get; set; }

        public DateTime? FechaDeModificacion { get; set; }

        [Required]
        public byte Estado { get; set; }

    }
}