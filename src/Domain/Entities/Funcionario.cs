using Domain.ValueObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Funcionario : EntityBase<Guid>
    {
        public string UserId { get; protected set; }
        public string Matricula { get; protected set; }
        public string Cargo { get; protected set; }
        public Nome Nome { get; protected set; }
        public Email Email { get; protected set; }
        public IEnumerable<Telefone> Telefones { get; protected set; }
        public Endereco EnderecoComercial { get; protected set; }
        public Endereco EnderecoResidencial { get; protected set; }
    }
}
