namespace BancoLosPatitos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inicial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bitacoras",
                c => new
                    {
                        IdEvento = c.Int(nullable: false, identity: true),
                        TablaDeEvento = c.String(nullable: false, maxLength: 20),
                        TipoDeEvento = c.String(nullable: false, maxLength: 20),
                        FechaDeEvento = c.DateTime(nullable: false),
                        Descripcion = c.String(nullable: false),
                        StackTrace = c.String(nullable: false),
                        DatosAnteriores = c.String(),
                        DatosPosteriores = c.String(),
                    })
                .PrimaryKey(t => t.IdEvento);
            
            CreateTable(
                "dbo.Cajas",
                c => new
                    {
                        IdCaja = c.Int(nullable: false, identity: true),
                        IdComercio = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Descripcion = c.String(nullable: false, maxLength: 150),
                        TelefonoSINPE = c.String(nullable: false, maxLength: 10),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        FechaDeModificacion = c.DateTime(nullable: false),
                        Estado = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.IdCaja)
                .ForeignKey("dbo.Comercios", t => t.IdComercio, cascadeDelete: true)
                .Index(t => t.IdComercio);
            
            CreateTable(
                "dbo.Comercios",
                c => new
                    {
                        IdComercio = c.Int(nullable: false, identity: true),
                        Identificacion = c.String(nullable: false, maxLength: 30),
                        TipoIdentificacion = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 200),
                        TipoDeComercio = c.Int(nullable: false),
                        Telefono = c.String(nullable: false, maxLength: 20),
                        CorreoElectronico = c.String(nullable: false, maxLength: 200),
                        Direccion = c.String(nullable: false, maxLength: 500),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        FechaDeModificacion = c.DateTime(nullable: false),
                        Estado = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.IdComercio);
            
            CreateTable(
                "dbo.Sinpes",
                c => new
                    {
                        IdSinpe = c.Int(nullable: false, identity: true),
                        TelefonoOrigen = c.String(nullable: false, maxLength: 10),
                        NombreOrigen = c.String(nullable: false, maxLength: 200),
                        TelefonoDestinatario = c.String(nullable: false, maxLength: 10),
                        NombreDestinatario = c.String(nullable: false, maxLength: 200),
                        Monto = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        Descripcion = c.String(maxLength: 50),
                        Estado = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.IdSinpe);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Cajas", "IdComercio", "dbo.Comercios");
            DropIndex("dbo.Cajas", new[] { "IdComercio" });
            DropTable("dbo.Sinpes");
            DropTable("dbo.Comercios");
            DropTable("dbo.Cajas");
            DropTable("dbo.Bitacoras");
        }
    }
}
