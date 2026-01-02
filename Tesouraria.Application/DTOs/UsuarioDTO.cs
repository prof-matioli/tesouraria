namespace Tesouraria.Application.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; } // Nome do perfil (ex: "Administrador")
        public bool IsAdmin { get; set; }  // Opcional: facilita lógica na tela
    }
}