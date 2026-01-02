using System;
using System.Threading.Tasks;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Utils; // Onde está o SenhaHelper
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task SalvarUsuarioAsync(Usuario usuario, string senhaPura)
        {
            // 1. Validação de Duplicidade de Email
            var usuarioExistente = await _usuarioRepository.GetByEmailAsync(usuario.Email);

            if (usuarioExistente != null && usuarioExistente.Id != usuario.Id)
            {
                throw new InvalidOperationException("Já existe um usuário cadastrado com este E-mail.");
            }

            // 2. Tratamento da Senha
            if (!string.IsNullOrEmpty(senhaPura))
            {
                // Se o usuário digitou uma senha, gera o Hash
                usuario.SenhaHash = PasswordHelper.GenerateHash(senhaPura);
            }
            else if (usuario.Id == 0)
            {
                // Se é novo usuário, a senha é obrigatória
                throw new InvalidOperationException("A senha é obrigatória para novos usuários.");
            }
            // Se for edição (Id > 0) e a senhaPura for vazia, mantemos o Hash antigo que já está no objeto.

            // 3. Persistência
            if (usuario.Id == 0)
            {
                usuario.DataCriacao = DateTime.Now;
                await _usuarioRepository.AddAsync(usuario);
            }
            else
            {
                usuario.DataAtualizacao = DateTime.Now;
                await _usuarioRepository.UpdateAsync(usuario);
            }
        }

        public async Task<UsuarioDTO> AutenticarAsync(string email, string senhaPura)
        {
            // 1. Busca a Entidade COMPLETA no banco (incluindo senha hash e perfil)
            var usuario = await _usuarioRepository.GetByEmailAsync(email);

            // Validações básicas
            if (usuario == null) return null;
            if (!usuario.Ativo) return null;

            // 2. Verifica a Senha
            bool senhaValida = PasswordHelper.VerifyPassword(senhaPura, usuario.SenhaHash);

            if (!senhaValida) return null;

            // 3. Mapeia para DTO (Retorna apenas o necessário)
            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil?.Nome, // Pega o nome do perfil (trata nulo com ?)

                // Exemplo de lógica simples: Se o perfil contiver "Admin", é admin
                IsAdmin = usuario.Perfil?.Nome?.ToLower().Contains("admin") ?? false
            };
        }
    }
}