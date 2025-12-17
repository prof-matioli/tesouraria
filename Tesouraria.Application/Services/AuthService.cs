using AutoMapper;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class AuthService : IAuthService
    {
        // Campos privados e readonly para garantir que não sejam alterados após o construtor
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        // Injeção de Dependência via Construtor
        // Recebemos a INTERFACE do repositório, não a implementação concreta.
        public AuthService(IUsuarioRepository usuarioRepository, IMapper mapper)
        {
            // Boa prática: "Fail Fast". Se a injeção falhar, o erro estoura aqui e não no método LoginAsync.
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<UsuarioDTO?> LoginAsync(string email, string senha)
        {
            // 1. Busca o usuário no banco pelo e-mail
            // (Ajuste conforme seu repositório de usuário)
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            // 2. Verifica se existe e se a senha bate
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
                return null; // Login falhou

            // 3. Se deu certo, converte para DTO e retorna
            return new UsuarioDTO
            {
                Id = usuario.Id, // <--- Aqui pegamos o ID real do banco (ex: 5, 10, etc)
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil
            };
        }
    }
}