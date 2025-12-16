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
            if (string.IsNullOrWhiteSpace(email)) return null;

            // O filtro acontece no banco de dados (SQL WHERE Email = ...), retornando apenas 1 registro.
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            if (usuario == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)) return null;

            return _mapper.Map<UsuarioDTO>(usuario);
        }
    }
}