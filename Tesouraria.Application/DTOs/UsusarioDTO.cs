using Tesouraria.Domain.Enums;

namespace Tesouraria.Application.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; } // <--- Essencial
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public PerfilUsuario Perfil { get; set; }
        public string Token { get; set; } = string.Empty; // Caso use JWT no futuro    }
    }
}