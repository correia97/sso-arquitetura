using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cadastro.API.Models.Response
{
    public class FuncionarioResponse
    {
        public FuncionarioResponse()
        {
            Telefones = new List<TelefoneResponse>();
        }
        public FuncionarioResponse(Funcionario funcionario)
        {
            Telefones = new List<TelefoneResponse>();

            Ativo = funcionario.Ativo;
            Cargo = funcionario.Cargo;
            DataNascimento = funcionario.DataNascimento?.Date;
            Email = funcionario.Email.ToString();
            Matricula = funcionario.Matricula;
            Nome = funcionario.Nome.PrimeiroNome;
            SobreNome = funcionario.Nome.SobreNome;
            UserId = funcionario.UserId;

            if (funcionario.Telefones != null && funcionario.Telefones.Any(x => x.Id > 0))
                Telefones = funcionario.Telefones
                    .Where(x => x.Id > 0)
                    .Select(tel => new TelefoneResponse
                    {
                        Id = tel.Id,
                        DDI = tel.DDI,
                        Telefone = $"{tel.DDD}{tel.NumeroTelefone}"
                    }).ToList();

            if (funcionario.EnderecoResidencial?.Id > 0)
                EnderecoResidencial = new EnderecoResponse
                {
                    Id = funcionario.EnderecoResidencial?.Id,
                    Rua = funcionario.EnderecoResidencial?.Rua,
                    Numero = funcionario.EnderecoResidencial?.Numero,
                    CEP = funcionario.EnderecoResidencial?.CEP,
                    Complemento = funcionario.EnderecoResidencial?.Complemento,
                    Bairro = funcionario.EnderecoResidencial?.Bairro,
                    Cidade = funcionario.EnderecoResidencial?.Cidade,
                    UF = funcionario.EnderecoResidencial?.UF,
                };


            if (funcionario.EnderecoComercial?.Id > 0)
                EnderecoComercial = new EnderecoResponse
                {
                    Id = funcionario.EnderecoComercial?.Id,
                    Rua = funcionario.EnderecoComercial?.Rua,
                    Numero = funcionario.EnderecoComercial?.Numero,
                    CEP = funcionario.EnderecoComercial?.CEP,
                    Complemento = funcionario.EnderecoComercial?.Complemento,
                    Bairro = funcionario.EnderecoComercial?.Bairro,
                    Cidade = funcionario.EnderecoComercial?.Cidade,
                    UF = funcionario.EnderecoComercial?.UF,
                };
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
