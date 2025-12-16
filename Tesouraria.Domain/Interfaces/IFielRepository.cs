using Tesouraria.Domain.Entities; // Ajuste para seu namespace de Entidades

namespace Tesouraria.Domain.Interfaces
{
    public interface IFielRepository
    {
        Task<List<Fiel>> ObterTodosAsync();
        Task<Fiel?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Fiel fiel);
        Task AtualizarAsync(Fiel fiel);
        Task RemoverAsync(int id);
    }
}