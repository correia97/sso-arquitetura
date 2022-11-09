using FluentMigrator;

namespace Cadastro.WorkerServices.Migrations
{
    [Migration(1)]
    public class CriarBaseMigration : Migration
    {
        public override void Up()
        {
            Create.Table("funcionarios")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("userid").AsString(50).NotNullable()
                .WithColumn("primeironome").AsString(150).NotNullable()
                .WithColumn("sobrenome").AsString(150).NotNullable()
                .WithColumn("matricula").AsString(50).Nullable()
                .WithColumn("cargo").AsString(150).Nullable()
                .WithColumn("enderecoemail").AsString(150).Nullable()
                .WithColumn("ativo").AsBoolean()
                .WithColumn("datacadastro").AsDateTime().NotNullable()
                .WithColumn("dataatualizacao").AsDateTime().Nullable();

            Create.Table("telefones")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("funcionarioid").AsGuid().ForeignKey("funcionarios", "id")
                .WithColumn("ddi").AsString(5).Nullable()
                .WithColumn("ddd").AsString(5).Nullable()
                .WithColumn("numerotelefone").AsString(10);

            Create.Table("enderecos")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("funcionarioid").AsGuid().ForeignKey("funcionarios", "id")
                .WithColumn("rua").AsString(250)
                .WithColumn("numero").AsInt32().Nullable()
                .WithColumn("cep").AsString(15)
                .WithColumn("complemento").AsString(20).Nullable()
                .WithColumn("bairro").AsString(150)
                .WithColumn("cidade").AsString(150)
                .WithColumn("uf").AsString(2);
        }
        public override void Down()
        {
            Delete.Table("funcionarios");

            Delete.Table("telefones");

            Delete.Table("enderecos");
        }
    }
}
