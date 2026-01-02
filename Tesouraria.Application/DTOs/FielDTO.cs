using System;
using System.ComponentModel.DataAnnotations;

namespace Tesouraria.Application.DTOs
{
    public class FielDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
        public string? CPF { get; set; }

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }

        public DateTime? DataNascimento { get; set; }

        public bool Dizimista { get; set; }
    }
}