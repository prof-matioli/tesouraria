namespace Tesouraria.Application.DTOs
{
    public class CentroCustoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        // Propriedade extra para exibição na Grid (ex: "1 - Administrativo")
        public string CodigoNome => $"{Id} - {Nome}";
    }
}