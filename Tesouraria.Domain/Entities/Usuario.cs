using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Common; 

namespace Tesouraria.Domain.Entities
{
    public class Usuario : BaseEntity
    {
        public string Nome { get;  set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty; // Nunca salvamos a senha real
        public PerfilUsuario Perfil { get; set; }
        public bool Ativo { get; set; }

        // Construtor vazio para o EF Core
        public Usuario() { }

        public Usuario(string nome, string email, string senhaHash, PerfilUsuario perfil)
        {
            Nome = nome;
            Email = email;
            SenhaHash = senhaHash;
            Perfil = perfil;
            Ativo = true;
        }

        public void AlterarSenha(string novoHash)
        {
            SenhaHash = novoHash;
        }
    }
}