using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Application.Interfaces
{
    public interface IUsuarioService
    {
        // Salvar cuida de Insert e Update, e do Hash da senha
        Task SalvarUsuarioAsync(Usuario usuario, string senhaPura);

        // Autenticar verifica email e senha
        Task<UsuarioDTO> AutenticarAsync(string email, string senhaPura);
    }
}