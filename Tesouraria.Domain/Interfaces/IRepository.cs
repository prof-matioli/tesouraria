// Tesouraria.Domain/Interfaces/IRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Common;

namespace Tesouraria.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id); // Implementaremos como Soft Delete (inativar)
    }
}