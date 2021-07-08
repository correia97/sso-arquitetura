using System;
using System.Text.RegularExpressions;

namespace Domain.ValueObject
{
    public class Email
    {
        private const string Pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        public Email(string email)
        {

            if (string.IsNullOrEmpty(email))
                throw new ArgumentException($"{email} não pode ser nulo", nameof(email));

            if (!Regex.IsMatch(email, Pattern))
                throw new ArgumentException($"{email} não é um endereço de e-mail válido", nameof(email));

            EnderecoEmail = email;
        }
        public string EnderecoEmail { get; protected set; }

        public override string ToString()
        {
            return EnderecoEmail;
        }
        public override bool Equals(object obj)
        {
            return obj is Email nome && this.ToString() == nome.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
