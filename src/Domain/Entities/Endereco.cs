namespace Domain.Entities
{
    public class Endereco : EntityBase<int>
    {
        public Endereco(string rua, int numero, string cep, string complemento, string bairro, string cidade, string uf)
        {
            Rua = rua;
            Numero = numero;
            CEP = cep;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
        }

        public Endereco(string rua, int numero, string cep, string bairro, string cidade, string uf)
        {
            Rua = rua;
            Numero = numero;
            CEP = cep;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
        }
        public Endereco(string rua, string cep, string complemento, string bairro, string cidade, string uf)
        {
            Rua = rua;
            CEP = cep;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
        }
        public Endereco(string rua, string cep, string bairro, string cidade, string uf)
        {
            Rua = rua;
            CEP = cep;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
        }
        public string Rua { get; protected set; }
        public int? Numero { get; protected set; }
        public string CEP { get; protected set; }
        public string Complemento { get; protected set; }
        public string Bairro { get; protected set; }
        public string Cidade { get; protected set; }
        public string UF { get; protected set; }
    }
}
