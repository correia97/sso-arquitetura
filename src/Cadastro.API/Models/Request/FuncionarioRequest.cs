using System;
using System.Collections.Generic;

namespace Cadastro.API.Models.Request
{
    public class FuncionarioRequest
    {
        public FuncionarioRequest()
        {
            Telefones = new List<TelefoneRequest>();
        }
        public string UserId { get; set; }
        public string Matricula { get; set; }
        public string Cargo { get; set; }
        public string Nome { get; set; }
        public string SobreNome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public List<TelefoneRequest> Telefones { get; set; }
        public EnderecoRequest EnderecoComercial { get; set; }
        public EnderecoRequest EnderecoResidencial { get; set; }
        public bool Ativo { get; set; }

    }
}