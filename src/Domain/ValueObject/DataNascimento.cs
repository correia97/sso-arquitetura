using System;

namespace Domain.ValueObject
{
    public class DataNascimento
    {
        public DataNascimento(DateTime date)
        {
            if (date > DateTime.Now)
                throw new ArgumentException("Data Futura", nameof(date));

            Date = date;
        }
        public DateTime Date { get; private set; }

        public override string ToString()
        {
            return Date.ToString("dd/MM/yyyy");
        }
        public override bool Equals(object obj)
        {
            return obj is DataNascimento nome && this.ToString() == nome.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
