using System;
using System.Collections.Generic;

namespace Cadastro.MVC.Models.Response
{
    public class FuncionarioResponse
    {
        public string UserId { get; set; }
        public string Matricula { get; set; }
        public string Cargo { get; set; }
        public string Nome { get; set; }
        public string SobreNome { get; set; }
        public string Email { get; set; }
        public DateTime? DataNascimento { get; set; }
        public List<TelefoneResponse> Telefones { get; set; } = new List<TelefoneResponse>();
        public EnderecoResponse EnderecoComercial { get; set; }
        public EnderecoResponse EnderecoResidencial { get; set; }
        public bool Ativo { get; set; }
    }
}
