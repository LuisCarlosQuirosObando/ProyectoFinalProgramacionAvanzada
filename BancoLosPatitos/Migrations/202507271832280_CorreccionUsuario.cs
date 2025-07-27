namespace BancoLosPatitos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorreccionUsuario : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Usuarios", "Estado", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Usuarios", "Estado", c => c.Boolean(nullable: false));
        }
    }
}
