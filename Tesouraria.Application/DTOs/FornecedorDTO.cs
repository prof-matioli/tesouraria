using System.ComponentModel.DataAnnotations;

namespace Tesouraria.Application.DTOs
{
    public class FornecedorDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A Razão Social é obrigatória.")]
        [StringLength(150)]
        public string RazaoSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome Fantasia é obrigatório.")]
        [StringLength(150)]
        public string NomeFantasia { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CNPJ ou CPF é obrigatório.")]
        [StringLength(20)]
        public string CNPJ_CPF { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}