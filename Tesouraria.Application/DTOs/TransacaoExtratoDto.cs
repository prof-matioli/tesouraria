using System;

namespace Tesouraria.Application.DTOs
{
    public class TransacaoExtratoDto
    {
        public DateTime Data { get; set; }
        public string Historico { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public char Tipo { get; set; } // 'C' = Crédito (Receita), 'D' = Débito (Despesa)

        // Propriedades auxiliares para facilitar a exibição na tela (DataGrid)
        public string TipoFormatado => Tipo == 'C' ? "Receita" : "Despesa";
        public bool IsReceita => Tipo == 'C';
    }
}