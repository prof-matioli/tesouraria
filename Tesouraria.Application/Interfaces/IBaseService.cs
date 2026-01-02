// Tesouraria.Application/Interfaces/IBaseService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tesouraria.Application.Interfaces
{
    public interface IBaseService<T, TDto>
    {
        Task<TDto> AddAsync(TDto dto);
        Task UpdateAsync(TDto dto);
        Task DeleteAsync(int id);
        Task<TDto?> GetByIdAsync(int id);
        Task<IEnumerable<TDto>> GetAllAsync();
    }
}