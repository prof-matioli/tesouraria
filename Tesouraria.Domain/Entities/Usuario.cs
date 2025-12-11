using Tesouraria.Domain.Enums;

namespace Tesouraria.Domain.Entities
{
    public class Usuario : Entity
    {
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string SenhaHash { get; private set; } = string.Empty; // Nunca salvamos a senha real
        public PerfilUsuario Perfil { get; private set; }
        public bool Ativo { get; private set; }

        // Construtor vazio para o EF Core
        protected Usuario() { }

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