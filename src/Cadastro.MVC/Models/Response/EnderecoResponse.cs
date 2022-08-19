using System.Text.Json.Serialization;

namespace Cadastro.MVC.Models.Response
{
    public class EnderecoResponse
    {
        [JsonConstructor]
        public EnderecoResponse(string rua, int? numero, string cep, string complemento, string bairro, string cidade, string uf)
        {
            Rua = rua;
            Numero = numero;
            CEP = cep;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            UF = uf;
        }

        public int Id { get; set; }
        public string Rua { get; set; }
        public int? Numero { get; set; }
        public string CEP { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string UF { get; set; }
    }
}
