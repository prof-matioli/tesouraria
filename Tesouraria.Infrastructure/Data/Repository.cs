// Tesouraria.Infra.Data/Repositories/Repository.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Common;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dataset;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dataset = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dataset.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.DataCriacao = DateTime.Now;
            entity.Ativo = true;
            await _dataset.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            entity.DataAtualizacao = DateTime.Now;
            _dataset.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dataset.FindAsync(id);

            if (entity != null)
            {
                // 1. Altera os valores
                entity.Ativo = false;
                entity.DataAtualizacao = DateTime.Now;

                // 2. O SEGREDO: Força o EF a reconhecer que houve mudança
                // Isso garante que o UPDATE será gerado mesmo se o rastreamento falhar
                _context.Entry(entity).State = EntityState.Modified;

                // Ou, se preferir uma sintaxe mais simples que faz a mesma coisa:
                // _dataset.Update(entity); 

                // 3. Salva no banco
                await _context.SaveChangesAsync();
            }
        }
    }
}