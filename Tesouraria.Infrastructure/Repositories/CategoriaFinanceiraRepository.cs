using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data; // Ajuste para onde está seu DbContext

namespace Tesouraria.Infrastructure.Repositories
{
    public class CategoriaFinanceiraRepository : ICategoriaFinanceiraRepository
    {
        private readonly AppDbContext _context;

        public CategoriaFinanceiraRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoriaFinanceira>> GetAllAsync()
        {
            // AsNoTracking melhora performance para leitura
            return await _context.Set<CategoriaFinanceira>()
                     .AsNoTracking() 
                     .ToListAsync();

        }

        public async Task<CategoriaFinanceira?> GetByIdAsync(int id)
        {
            return await _context.Set<CategoriaFinanceira>().FindAsync(id);
        }

        public async Task AddAsync(CategoriaFinanceira entity)
        {
            await _context.Set<CategoriaFinanceira>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoriaFinanceira entity)
        {
            _context.Set<CategoriaFinanceira>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<CategoriaFinanceira>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}