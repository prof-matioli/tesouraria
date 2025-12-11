using Tesouraria.Domain.Entities;

namespace Tesouraria.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario?> LoginAsync(string email, string senha);
    }
}