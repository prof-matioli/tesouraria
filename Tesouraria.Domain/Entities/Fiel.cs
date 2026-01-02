// Tesouraria.Domain/Entities/Fiel.cs
using Tesouraria.Domain.Common;

namespace Tesouraria.Domain.Entities
{
    public class Fiel : BaseEntity
    {
        public string Nome { get; set; } = string.Empty;
        public string? CPF { get; set; } // Opcional
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? Endereco { get; set; }
        public DateTime? DataNascimento { get; set; }
        public bool Dizimista { get; set; }
    }
}