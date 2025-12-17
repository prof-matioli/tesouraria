using Microsoft.EntityFrameworkCore;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infra.Data.Repositories
{
    public class LancamentoRepository : ILancamentoRepository
    {
        private readonly AppDbContext _context;

        public LancamentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lancamento?> ObterPorIdAsync(int id)
        {
            return await _context.Lancamento
                .Include(l => l.Categoria)
                .Include(l => l.CentroCusto)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AdicionarAsync(Lancamento lancamento)
        {
            await _context.Lancamento.AddAsync(lancamento);
        }

        public Task AtualizarAsync(Lancamento lancamento)
        {
            _context.Lancamento.Update(lancamento);
            return Task.CompletedTask; // O EF Core rastreia mudanças, o update é marcado no contexto
        }

        public async Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            return await _context.Lancamento
                .AsNoTracking() // Performance para leitura
                .Include(l => l.Categoria)
                .Include(l => l.CentroCusto)
                .Include(l => l.Fiel)
                .Include(l => l.Fornecedor)
                .Where(l => l.DataVencimento >= inicio && l.DataVencimento <= fim)
                .OrderBy(l => l.DataVencimento)
                .ToListAsync();
        }

        public async Task<decimal> ObterTotalPrevistoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo)
        {
            // Soma o Valor Original (previsto) de tudo que vence no período e NÃO está cancelado.
            // Note que incluímos os Pagos e os Pendentes aqui.
            return await _context.Lancamento
                .Where(l => l.Status != StatusLancamento.Cancelado
                            && l.DataVencimento >= inicio
                            && l.DataVencimento <= fim
                            && l.Tipo == tipo)
                .SumAsync(l => l.ValorOriginal);
        }
        public async Task<decimal> ObterTotalPorPeriodoETipoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo)
        {
            return await _context.Lancamento
                .Where(l => l.Status == StatusLancamento.Pago
                            && l.DataPagamento >= inicio
                            && l.DataPagamento <= fim
                            && l.Tipo == tipo)
                .SumAsync(l => l.ValorPago);
        }

        public async Task<bool> CommitAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}