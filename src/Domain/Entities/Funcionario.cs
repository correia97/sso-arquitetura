using Domain.ValueObject;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Funcionario : EntityBase<Guid>
    {
        public Funcionario(string userId, string matricula, string cargo, Nome nome, Email email)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Matricula = matricula;
            Cargo = cargo;
            Nome = nome;
            Email = email;
            DataCadastro = DateTime.Now;
        }
        public Funcionario(string userId, string matricula, string cargo, Nome nome, Email email, 
            IEnumerable<Telefone> telefones, Endereco enderecoResidencial,  Endereco enderecoComercial)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Matricula = matricula;
            Cargo = cargo;
            Nome = nome;
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
        public IEnumerable<Telefone> Telefones { get; protected set; }
        public Endereco EnderecoComercial { get; protected set; }
        public Endereco EnderecoResidencial { get; protected set; }
        public DateTime DataCadastro { get; protected set; }
        public DateTime? DataAtualizacao { get; protected set; }
        public bool Ativo { get; protected set; }

        public void Atualizar(Nome nome, Email email, string matricula, string cargo)
        {
            Nome = nome;
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
    }
}
