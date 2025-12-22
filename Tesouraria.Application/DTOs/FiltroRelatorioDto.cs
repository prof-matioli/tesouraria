using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Application.DTOs
{
    public class FiltroRelatorioDto
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int? CentroCustoId { get; set; }
        public TipoTransacao? Tipo { get; set; } // Null = Todos
        public bool ApenasPagos { get; set; } = true; // Geralmente fluxo de caixa é só o realizado
    }
}