namespace Cadastro.MVC.Models.Response
{
    public class TelefoneResponse
    {
        public TelefoneResponse(string ddi, string telefone)
        {
            DDI = ddi;
            Telefone = telefone;
        }
        public int Id { get; set; }
        public string DDI { get; set; }
        public string Telefone { get; set; }
    }
}
