using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cadastro.MVC.Models.Response
{
    public class FuncionarioResponse
    {
        [JsonConstructor]
        public FuncionarioResponse(string userId, string matricula, string cargo, string nome,
            string sobreNome, string email, DateTime? dataNascimento, bool ativo, List<TelefoneResponse> telefones, 
            EnderecoResponse enderecoComercial, EnderecoResponse enderecoResidencial)
        {
            UserId = userId;
            Matricula = matricula;
            Cargo = cargo;
            Nome = nome;
            SobreNome = sobreNome;
            Email = email;
            DataNascimento = dataNascimento;
            Ativo = ativo;
            Telefones = telefones;
            EnderecoComercial = enderecoComercial;
            EnderecoResidencial = enderecoResidencial;
        }
        public FuncionarioResponse()
        {
            Telefones = new List<TelefoneResponse>();
        }
        public string UserId { get; set; }
        public string Matricula { get; set; }
        public string Cargo { get; set; }
        public string Nome { get; set; }
        public string SobreNome { get; set; }
        public string Email { get; set; }
        public DateTime? DataNascimento { get; set; }
        public List<TelefoneResponse> Telefones { get; set; }
        public EnderecoResponse EnderecoComercial { get; set; }
        public EnderecoResponse EnderecoResidencial { get; set; }
        public bool Ativo { get; set; }
    }
}
