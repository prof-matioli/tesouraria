using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Infrastructure.Data.Repositories
{
    public class CentroCustoRepository : ICentroCustoRepository
    {
        private readonly AppDbContext _context;

        public CentroCustoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CentroCusto>> ObterTodosAsync()
        {
            return await _context.Set<CentroCusto>()
                                 .AsNoTracking() // Performance
                                 .ToListAsync();
        }

        public async Task<CentroCusto?> ObterPorIdAsync(int id)
        {
            return await _context.Set<CentroCusto>().FindAsync(id);
        }

        public async Task AdicionarAsync(CentroCusto entity)
        {
            await _context.Set<CentroCusto>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(CentroCusto entity)
        {
            _context.Set<CentroCusto>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            var entity = await ObterPorIdAsync(id);
            if (entity != null)
            {
                _context.Set<CentroCusto>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}