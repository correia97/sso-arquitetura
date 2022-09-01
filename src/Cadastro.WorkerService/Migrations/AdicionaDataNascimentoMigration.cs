using FluentMigrator;

namespace Cadastro.WorkerService.Migrations
{
    [Migration(2)]
    public class AdicionaDataNascimentoMigration : Migration
    {
        public override void Up()
        {
            Create.Column("datanascimento").OnTable("funcionarios").AsDateTime().Nullable();
        }
        public override void Down()
        {
            Delete.Column("datanascimento").FromTable("funcionarios");
        }
    }
}
