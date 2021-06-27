using System;

namespace Domain.ValueObject
{
    public class Nome
    {
        public Nome(string primeiroNome, string sobreNome)
        {
            if (string.IsNullOrEmpty(primeiroNome))
                throw new ArgumentException($"{primeiroNome} não pode ser nulo", nameof(primeiroNome));

            if (string.IsNullOrEmpty(sobreNome))
                throw new ArgumentException($"{sobreNome} não pode ser nulo", nameof(sobreNome));

            PrimeiroNome = primeiroNome;
            SobreNome = sobreNome;
        }

        public string PrimeiroNome { get; protected set; }
        public string SobreNome { get; protected set; }
        public override string ToString()
        {
            return $"{PrimeiroNome} {SobreNome}".Trim();
        }
        public override bool Equals(object obj)
        {
            return obj is Nome nome && this.ToString() == nome.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
