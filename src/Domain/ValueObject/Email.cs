using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Domain.ValueObject
{
    public class Email
    {
        private const string Pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        protected Email()
        {

        }
        [JsonConstructor]
        public Email(string enderecoEmail)
        {

            if (string.IsNullOrEmpty(enderecoEmail))
                throw new ArgumentException($"{enderecoEmail} não pode ser nulo", nameof(enderecoEmail));

            if (!Regex.IsMatch(enderecoEmail, Pattern))
                throw new ArgumentException($"{enderecoEmail} não é um endereço de e-mail válido", nameof(enderecoEmail));

            EnderecoEmail = enderecoEmail;
        }
        public string EnderecoEmail { get; protected set; }

        public override string ToString()
        {
            return EnderecoEmail;
        }
        public override bool Equals(object obj)
        {
            return obj is Email email && this.ToString() == email.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
