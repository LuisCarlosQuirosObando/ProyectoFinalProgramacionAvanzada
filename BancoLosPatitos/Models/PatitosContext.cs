using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace BancoLosPatitos.Models
{
    public class PatitosContext : DbContext
    {
        public PatitosContext() : base("BancoLosPatitosDbConnection")
        {

        }
        public DbSet<Comercio> Comercios { get; set; }

        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Sinpe> Sinpes { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }

        public DbSet<ConfiguracionComercio> ConfiguracionComercios { get; set; }

        public DbSet<Reporte> Reportes { get; set; }

        public DbSet<Usuarios> Usuarios { get; set; }

    }
}