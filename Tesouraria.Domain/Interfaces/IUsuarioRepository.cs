using Tesouraria.Domain.Entities;

namespace Tesouraria.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorEmailAsync(string email);
    }
}