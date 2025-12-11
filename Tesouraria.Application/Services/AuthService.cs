using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        // Injeção de Dependência via Construtor
        // Recebemos a INTERFACE do repositório, não a implementação concreta.
        public AuthService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario?> LoginAsync(string email, string senha)
        {
            // 1. Busca o usuário no banco de dados pelo e-mail
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            // 2. Validações iniciais (se não existe ou está inativo, retorna null)
            if (usuario == null || !usuario.Ativo)
            {
                return null;
            }

            // 3. Verifica a senha
            // O BCrypt compara a senha digitada (texto puro) com o Hash salvo no banco
            bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash);

            if (senhaValida)
            {
                return usuario; // Autenticação bem-sucedida
            }

            return null; // Senha incorreta
        }
    }
}