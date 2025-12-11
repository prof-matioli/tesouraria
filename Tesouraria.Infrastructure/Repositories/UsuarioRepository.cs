using Microsoft.EntityFrameworkCore;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}