using System.ComponentModel.DataAnnotations;

namespace Tesouraria.Application.DTOs
{
    public class CentroCustoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do centro de custo é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descricao { get; set; }
    }
}