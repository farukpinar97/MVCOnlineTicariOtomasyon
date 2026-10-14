namespace MVCOnlineTicariOtomasyon.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Messages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        Subject = c.String(maxLength: 100, unicode: false),
                        Content = c.String(maxLength: 2000, unicode: false),
                        Date = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReceiverId = c.Int(),
                        SenderId = c.Int(),
                    })
                .PrimaryKey(t => t.MessageId)
                .ForeignKey("dbo.Currents", t => t.ReceiverId)
                .ForeignKey("dbo.Currents", t => t.SenderId)
                .Index(t => t.ReceiverId)
                .Index(t => t.SenderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "SenderId", "dbo.Currents");
            DropForeignKey("dbo.Messages", "ReceiverId", "dbo.Currents");
            DropIndex("dbo.Messages", new[] { "SenderId" });
            DropIndex("dbo.Messages", new[] { "ReceiverId" });
            DropTable("dbo.Messages");
        }
    }
}
