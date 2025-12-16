// Tesouraria.Infra.Data/Repositories/Repository.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesouraria.Domain.Common;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infra.Data.Repositories
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

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Retorna apenas os ativos
            return await _dataset.Where(x => x.Ativo).ToListAsync();
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
                entity.Ativo = false; // Soft Delete
                entity.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}