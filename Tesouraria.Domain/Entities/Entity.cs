namespace Tesouraria.Domain.Entities
{
    public abstract class Entity
    {

        public int Id { get; protected set; }
        // Auditabilidade básica
        public DateTime DataCriacao { get; set; }

        protected Entity()
        {
            DataCriacao = DateTime.Now;
        }
    }
}