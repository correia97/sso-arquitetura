namespace Domain.ValueObject
{
    public  class Telefone
    {
        public Telefone(string ddi, string ddd, string numeroTelefone)
        {
            DDI = ddi;
            DDD = ddd;
            NumeroTelefone = numeroTelefone;
        }

        public string NumeroTelefone { get; private set; }
        public string DDD { get; private set; }
        public string DDI { get; private set; }

        public override string ToString()
        {
            return $"{DDI} ({DDD}) {NumeroTelefone}".Trim();
        }
        public override bool Equals(object obj)
        {
            return obj is Telefone nome && this.ToString() == nome.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
