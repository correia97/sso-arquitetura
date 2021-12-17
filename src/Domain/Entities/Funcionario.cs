using Domain.ValueObject;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Funcionario : EntityBase<Guid>
    {
        protected Funcionario()
        {

        }

        public Funcionario(string userId, string matricula, string cargo, Nome nome, DataNascimento dataNascimento, Email email)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Matricula = matricula;
            Cargo = cargo;
            Nome = nome;
            DataNascimento = dataNascimento;
            Email = email;
            DataCadastro = DateTime.Now;
        }
        [JsonConstructor]
        public Funcionario(string userId, string matricula, string cargo, Nome nome, DataNascimento dataNascimento, Email email,
            IEnumerable<Telefone> telefones, Endereco enderecoResidencial, Endereco enderecoComercial)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Matricula = matricula;
            Cargo = cargo;
            Nome = nome;
            DataNascimento = dataNascimento;
            Email = email;
            EnderecoResidencial = enderecoResidencial;
            Telefones = telefones;
            EnderecoComercial = enderecoComercial;
            DataCadastro = DateTime.Now;

        }

        public string UserId { get; protected set; }
        public string Matricula { get; protected set; }
        public string Cargo { get; protected set; }
        public Nome Nome { get; protected set; }
        public Email Email { get; protected set; }
        public DataNascimento DataNascimento { get; protected set; }
        public IEnumerable<Telefone> Telefones { get; protected set; }
        public Endereco EnderecoComercial { get; protected set; }
        public Endereco EnderecoResidencial { get; protected set; }
        public DateTime DataCadastro { get; protected set; }
        public DateTime? DataAtualizacao { get; protected set; }
        public bool Ativo { get; protected set; }

        public void Atualizar(Nome nome, DataNascimento dataNascimento, Email email, string matricula, string cargo)
        {
            Nome = nome;
            DataNascimento = dataNascimento;
            Email = email;
            Matricula = matricula;
            Cargo = cargo;
            DataAtualizacao = DateTime.Now;
        }

        public void AtualizarTelefones(IEnumerable<Telefone> telefones)
        {
            Telefones = telefones;
        }

        public void AtualizarEnderecoComercial(Endereco enderecoComercial)
        {
            EnderecoComercial = enderecoComercial;
        }

        public void AtualizarEnderecoResidencial(Endereco enderecoResidencial)
        {
            EnderecoResidencial = enderecoResidencial;
        }

        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
