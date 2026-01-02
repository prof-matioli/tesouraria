using Microsoft.EntityFrameworkCore;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data; // Ajuste para onde está seu DbContext

namespace Tesouraria.Infrastructure.Repositories
{
    public class FielRepository : IFielRepository
    {
        private readonly AppDbContext _context; // Substitua 'SeuDbContext' pelo nome real do seu contexto

        public FielRepository(AppDbContext context)
        {
            _context = context;
        }

        // --- AQUI ESTÁ A SOLUÇÃO DO SEU ERRO DE RASTREAMENTO ---
        public async Task<List<Fiel>> ObterTodosAsync()
        {
            return await _context.Fieis
                                 .AsNoTracking() // <--- O PULO DO GATO: Lê sem vigiar.
                                 .ToListAsync();
        }

        public async Task<Fiel?> ObterPorIdAsync(int id)
        {
            return await _context.Fieis.FindAsync(id);
        }

        public async Task AdicionarAsync(Fiel fiel)
        {
            await _context.Fieis.AddAsync(fiel);
            await _context.SaveChangesAsync();
        }

        // --- VERSÃO BLINDADA DO UPDATE ---
        public async Task AtualizarAsync(Fiel fiel)
        {
            // Verificação de segurança: Se por acaso o objeto já estiver sendo vigiado pelo EF
            // (pode acontecer se você carregou ele em outra parte da tela sem AsNoTracking)
            var local = _context.Fieis.Local.FirstOrDefault(entry => entry.Id.Equals(fiel.Id));

            // Se achou um "gêmeo" na memória, solta ele para evitar o erro.
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            // Agora anexa o objeto novo que veio da edição e marca como Modificado
            _context.Entry(fiel).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var fiel = await ObterPorIdAsync(id);
            if (fiel != null)
            {
                _context.Fieis.Remove(fiel);
                await _context.SaveChangesAsync();
            }
        }
    }
}