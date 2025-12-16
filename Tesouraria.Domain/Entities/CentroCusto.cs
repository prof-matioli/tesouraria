// Tesouraria.Domain/Entities/CentroCusto.cs
using Tesouraria.Domain.Common;

namespace Tesouraria.Domain.Entities
{
    public class CentroCusto : BaseEntity
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }
}