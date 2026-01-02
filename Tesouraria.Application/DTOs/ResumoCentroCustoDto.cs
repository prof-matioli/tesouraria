namespace Tesouraria.Application.DTOs
{
    public class ResumoCentroCustoDto
    {
        public string CentroCusto { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;
    }
}