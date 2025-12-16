using System.ComponentModel.DataAnnotations;
using Tesouraria.Domain.Entities; // Necessário para acessar o Enum TipoTransacao

namespace Tesouraria.Application.DTOs
{
    public class CategoriaFinanceiraDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de transação é obrigatório.")]
        public TipoTransacao Tipo { get; set; }

        public bool DedutivelIR { get; set; }

        // Propriedade auxiliar apenas para leitura na Grid (opcional, facilita a exibição)
        public string TipoDescricao => Tipo == TipoTransacao.Receita ? "Receita" : "Despesa";
    }
}