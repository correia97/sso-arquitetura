using FluentMigrator;

namespace Cadastro.WorkerService.Migrations
{
    [Migration(3)]
    public class AdicionaTipoEndereco : Migration
    {
        public override void Up()
        {
            Create.Column("tipoendereco").OnTable("enderecos").AsInt32();
        }
        public override void Down()
        {
            Delete.Column("tipoendereco").FromTable("enderecos");
        }
    }
}
