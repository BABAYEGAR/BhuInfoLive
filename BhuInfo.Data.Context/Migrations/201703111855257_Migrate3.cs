namespace BhuInfo.Data.Context.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migrate3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Videos",
                c => new
                    {
                        VideoId = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Link = c.String(),
                        CreatedById = c.Long(nullable: false),
                        LastModifiedById = c.Long(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.VideoId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Videos");
        }
    }
}
