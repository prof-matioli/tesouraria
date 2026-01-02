using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Common; 

namespace Tesouraria.Domain.Entities
{
    public class Usuario : BaseEntity
    {
        public string Nome { get;  set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty; // Nunca salvamos a senha real
        public int PerfilId { get; set; }      // Chave Estrangeira (int)
        public virtual Perfil Perfil { get; set; } // Navegação (Objeto) - Apenas UMA vez

        // Construtor vazio para o EF Core
        public Usuario() { }

        public Usuario(string nome, string email, string senhaHash, int perfilId)
        {
            Nome = nome;
            Email = email;
            SenhaHash = senhaHash;
            PerfilId = perfilId;
            Ativo = true;
        }

        public void AlterarSenha(string novoHash)
        {
            SenhaHash = novoHash;
        }
    }
}