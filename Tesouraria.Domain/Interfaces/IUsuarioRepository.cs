using Tesouraria.Domain.Entities;

namespace Tesouraria.Domain.Interfaces
{
    public interface IUsuarioRepository: IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email); // Fundamental para o Login
        Task DeleteAsync(Usuario usuario); // Geralmente deletamos passando a entidade rastreada
    }
}