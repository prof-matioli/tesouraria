using Tesouraria.Domain.Entities; // Para acessar o Enum TipoTransacao

namespace Tesouraria.Application.DTOs
{
    public class CategoriaFinanceiraDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public TipoTransacao Tipo { get; set; } // Enum: Receita ou Despesa
        public bool DedutivelIR { get; set; }
    }
}