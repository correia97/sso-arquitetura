namespace Domain.Entities
{
    /// <summary>
    /// Base das entidades
    /// </summary>
    /// <typeparam name="U">Tipo de dado do ID</typeparam>
    public abstract class EntityBase<U>
    {
        protected EntityBase()
        {
        }
        public U Id { get; protected set; }

        public abstract string ToJson();
    }
}
