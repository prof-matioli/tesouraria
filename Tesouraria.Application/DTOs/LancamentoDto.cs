using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Application.DTOs
{
    public class LancamentoDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorOriginal { get; set; }
        public decimal? ValorPago { get; set; } // Usar nullable para saber se já foi pago
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public TipoTransacao Tipo { get; set; }
        public StatusLancamento Status { get; set; }
        public string? Observacao { get; set; } // Adicionado para edição

        // IDs para vinculação (Binding) nos ComboBoxes de Edição
        public int CategoriaId { get; set; }
        public int CentroCustoId { get; set; }
        public int? FielId { get; set; }
        public int? FornecedorId { get; set; }

        // Nomes para exibição no DataGrid (Read-only)
        public string CategoriaNome { get; set; } = string.Empty;
        public string CentroCustoNome { get; set; } = string.Empty;
        public string? PessoaNome { get; set; }

        public bool FiltrarPorDataPagamento { get; set; }
    }

    // As outras classes (CriarLancamentoDto, BaixarLancamentoDto) permanecem iguais
    public class CriarLancamentoDto
    {
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public TipoTransacao Tipo { get; set; }
        public int CategoriaId { get; set; }
        public int CentroCustoId { get; set; }
        public int UsuarioId { get; set; }
        public string? Observacao { get; set; }
        public int? FielId { get; set; }
        public int? FornecedorId { get; set; }
    }

    public class BaixarLancamentoDto
    {
        public int LancamentoId { get; set; }
        public decimal ValorPago { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}