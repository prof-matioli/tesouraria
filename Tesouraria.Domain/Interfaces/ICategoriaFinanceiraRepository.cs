using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Domain.Interfaces
{
    public interface ICategoriaFinanceiraRepository
    {
        Task<IEnumerable<CategoriaFinanceira>> ObterTodosAsync();
        Task<CategoriaFinanceira?> ObterPorIdAsync(int id);
        Task AdicionarAsync(CategoriaFinanceira entity);
        Task AtualizarAsync(CategoriaFinanceira entity);
        Task ExcluirAsync(int id);
    }
}