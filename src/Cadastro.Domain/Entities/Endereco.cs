using Cadastro.Domain.Enums;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Endereco : EntityBase<int>
    {
        protected Endereco()
        {
        }

        public Endereco(string rua, int? numero, string cep, string complemento, string bairro,
            string cidade, string uf, TipoEnderecoEnum tipoEndereco, Guid funcionarioId)
        {
            Rua = rua;
            Numero = numero;
            CEP = cep;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
            TipoEndereco = tipoEndereco;
            FuncionarioId = funcionarioId;
        }

        [JsonConstructor]
        public Endereco(int id, string rua, int? numero, string cep, string complemento, string bairro,
            string cidade, string uf, TipoEnderecoEnum tipoEndereco, Guid funcionarioId)
        {
            Id = id;
            Rua = rua;
            Numero = numero;
            CEP = cep;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
            TipoEndereco = tipoEndereco;
            FuncionarioId = funcionarioId;
        }

        public string Rua { get; protected set; }
        public int? Numero { get; protected set; }
        public string CEP { get; protected set; }
        public string Complemento { get; protected set; }
        public string Bairro { get; protected set; }
        public string Cidade { get; protected set; }
        public string UF { get; protected set; }
        public TipoEnderecoEnum TipoEndereco { get; set; }
        public Guid FuncionarioId { get; set; }
        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public override string ToString()
        {
            return $"{Rua}, {Numero}, {Bairro}, {Cidade} - {UF}".Trim();
        }
        public override bool Equals(object obj)
        {
            return obj is Endereco endereco && this.ToString() == endereco.ToString();
        }
        public override int GetHashCode()
        {
            return this.ToString().Length;
        }
    }
}
