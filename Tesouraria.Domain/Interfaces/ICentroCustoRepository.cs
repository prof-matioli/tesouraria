using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Domain.Interfaces
{
    public interface ICentroCustoRepository
    {
        Task<IEnumerable<CentroCusto>> ObterTodosAsync();
        Task<CentroCusto?> ObterPorIdAsync(int id);
        Task AdicionarAsync(CentroCusto entity);
        Task AtualizarAsync(CentroCusto entity);
        Task ExcluirAsync(int id);
    }
}