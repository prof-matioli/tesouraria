using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Domain.Interfaces
{
    public interface ICategoriaFinanceiraRepository
    {
        Task<IEnumerable<CategoriaFinanceira>> GetAllAsync();
        Task<CategoriaFinanceira?> GetByIdAsync(int id);
        Task AddAsync(CategoriaFinanceira entity);
        Task UpdateAsync(CategoriaFinanceira entity);
        Task DeleteAsync(int id);
    }
}