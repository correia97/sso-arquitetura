using System;
using System.Text.Json.Serialization;

namespace Domain.ValueObject
{
    public class DataNascimento
    {
        protected DataNascimento()
        {

        }

        [JsonConstructor]
        public DataNascimento(DateTime date)
        {
            if (date > DateTime.Now)
                throw new ArgumentException("Data Futura", nameof(date));

            Date = date;
        }
        public DateTime Date { get; protected set; }

        public override string ToString()
        {
            return Date.ToString("dd/MM/yyyy");
        }
        public override bool Equals(object obj)
        {
            return obj is DataNascimento data && this.ToString() == data.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
