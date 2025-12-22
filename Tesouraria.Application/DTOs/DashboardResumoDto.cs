namespace Tesouraria.Application.DTOs
{
    public class DashboardResumoDto
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;

        // Bônus: Comparativo (Exemplo simples)
        public string ComparativoReceita { get; set; } = "Calculando...";
        public string ComparativoDespesa { get; set; } = "Calculando...";

        // Nova propriedade para o gráfico
        public List<GraficoPontoDto> Historico { get; set; } = new List<GraficoPontoDto>();

        public class GraficoPontoDto
        {
            public string Mes { get; set; }      // Ex: "JAN", "FEV"
            public decimal Receitas { get; set; }
            public decimal Despesas { get; set; }
        }
    }
}