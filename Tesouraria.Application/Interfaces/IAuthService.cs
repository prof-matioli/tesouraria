using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UsuarioDTO?> LoginAsync(string email, string senha);
    }
}