namespace Datos.Persistencia.Core
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fase1ControlChangeMigration : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.ACH_PaybankReturnFileConfiguration");
            AddColumn("dbo.ACH_PaybankReturnFileConfiguration", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.ACH_PaybankReturnFileConfiguration", "CurrencyId", c => c.Int(nullable: false, defaultValue: 1));
            AddColumn("dbo.ACH_PostAccountingFlexcubeConfiguration", "CurrencyOriginalId", c => c.Int(nullable: false, defaultValue: 1));
            AddColumn("dbo.ACH_PostPaylinkConfiguration", "CurrencyId", c => c.Int(nullable: false, defaultValue: 1));
            AddPrimaryKey("dbo.ACH_PaybankReturnFileConfiguration", "Id");
            CreateIndex("dbo.ACH_PaybankReturnFileConfiguration", "CurrencyId");
            CreateIndex("dbo.ACH_PostPaylinkConfiguration", "CurrencyId");
            CreateIndex("dbo.ACH_PostAccountingFlexcubeConfiguration", "CurrencyOriginalId");
            AddForeignKey("dbo.ACH_PaybankReturnFileConfiguration", "CurrencyId", "dbo.ACH_Currency", "Id");
            AddForeignKey("dbo.ACH_PostPaylinkConfiguration", "CurrencyId", "dbo.ACH_Currency", "Id");
            AddForeignKey("dbo.ACH_PostAccountingFlexcubeConfiguration", "CurrencyOriginalId", "dbo.ACH_Currency", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ACH_PostAccountingFlexcubeConfiguration", "CurrencyOriginalId", "dbo.ACH_Currency");
            DropForeignKey("dbo.ACH_PostPaylinkConfiguration", "CurrencyId", "dbo.ACH_Currency");
            DropForeignKey("dbo.ACH_PaybankReturnFileConfiguration", "CurrencyId", "dbo.ACH_Currency");
            DropIndex("dbo.ACH_PostAccountingFlexcubeConfiguration", new[] { "CurrencyOriginalId" });
            DropIndex("dbo.ACH_PostPaylinkConfiguration", new[] { "CurrencyId" });
            DropIndex("dbo.ACH_PaybankReturnFileConfiguration", new[] { "CurrencyId" });
            DropPrimaryKey("dbo.ACH_PaybankReturnFileConfiguration");
            DropColumn("dbo.ACH_PostPaylinkConfiguration", "CurrencyId");
            DropColumn("dbo.ACH_PostAccountingFlexcubeConfiguration", "CurrencyOriginalId");
            DropColumn("dbo.ACH_PaybankReturnFileConfiguration", "CurrencyId");
            DropColumn("dbo.ACH_PaybankReturnFileConfiguration", "Id");
            AddPrimaryKey("dbo.ACH_PaybankReturnFileConfiguration", "CountryId");
        }
    }
}
