using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infra.Data.Repositories
{
    public class CategoriaFinanceiraRepository : ICategoriaFinanceiraRepository
    {
        private readonly AppDbContext _context;

        public CategoriaFinanceiraRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoriaFinanceira>> ObterTodosAsync()
        {
            return await _context.Set<CategoriaFinanceira>()
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<CategoriaFinanceira?> ObterPorIdAsync(int id)
        {
            return await _context.Set<CategoriaFinanceira>().FindAsync(id);
        }

        public async Task AdicionarAsync(CategoriaFinanceira entity)
        {
            await _context.Set<CategoriaFinanceira>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(CategoriaFinanceira entity)
        {
            _context.Set<CategoriaFinanceira>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            var entity = await ObterPorIdAsync(id);
            if (entity != null)
            {
                _context.Set<CategoriaFinanceira>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}