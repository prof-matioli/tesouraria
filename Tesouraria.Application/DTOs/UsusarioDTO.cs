namespace Tesouraria.Application.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;

        // Nota: Por segurança, não retornamos a Senha/Hash neste DTO 
        // usado para trafegar dados do usuário logado na tela.
    }
}