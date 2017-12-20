namespace CornerkickWebMvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Vorname", c => c.String());
            AddColumn("dbo.AspNetUsers", "Nachname", c => c.String());
            AddColumn("dbo.AspNetUsers", "Vereinsname", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Vereinsname");
            DropColumn("dbo.AspNetUsers", "Nachname");
            DropColumn("dbo.AspNetUsers", "Vorname");
        }
    }
}
