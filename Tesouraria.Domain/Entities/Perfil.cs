using Tesouraria.Domain.Common;

namespace Tesouraria.Domain.Entities
{
    public class Perfil : BaseEntity
    {
        public string Nome { get; set; } // Ex: Administrador, Tesoureiro
        public string Descricao { get; set; }
    }
}