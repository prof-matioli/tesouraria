// Tesouraria.Domain/Entities/CategoriaFinanceira.cs
using Tesouraria.Domain.Common;

namespace Tesouraria.Domain.Entities
{
    public enum TipoTransacao { Receita, Despesa }

    public class CategoriaFinanceira : BaseEntity
    {
        public string Nome { get; set; } = string.Empty;
        public TipoTransacao Tipo { get; set; }
        public bool DedutivelIR { get; set; } // Ex: Doações podem ser dedutíveis
    }
}