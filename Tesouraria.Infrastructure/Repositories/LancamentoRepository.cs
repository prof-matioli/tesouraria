using Microsoft.EntityFrameworkCore;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infrastructure.Repositories
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
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, bool incluirCancelados)
        {
            var query = _context.Lancamento
                .AsNoTracking()
                .Include(l => l.Categoria)
                .Include(l => l.CentroCusto)
                .Include(l => l.Fiel)
                .Include(l => l.Fornecedor)
                .Where(l => l.DataVencimento >= inicio && l.DataVencimento <= fim);

            if (!incluirCancelados)
            {
                query = query.Where(l => l.Status != StatusLancamento.Cancelado);
            }

            return await query
                .OrderBy(l => l.DataVencimento)
                .ToListAsync();
        }

        public async Task<decimal> ObterTotalPrevistoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo)
        {
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

        // === CORREÇÃO PRINCIPAL AQUI ===
        public async Task<IEnumerable<Lancamento>> ObterFiltradosAsync(
                                    DateTime inicio,
                                    DateTime fim,
                                    int? centroCustoId,
                                    TipoTransacao? tipo,
                                    bool apenasPagos,
                                    bool incluirCancelados,
                                    bool filtrarPorPagamento)
        {
            var query = _context.Lancamento
                .AsNoTracking()
                .Include(l => l.Categoria)
                .Include(l => l.CentroCusto)
                .Include(l => l.Fiel)
                .Include(l => l.Fornecedor)
                .AsQueryable();

            // 1. Filtro de Período
            if (filtrarPorPagamento)
            {
                query = query.Where(l => l.DataPagamento >= inicio && l.DataPagamento <= fim);
            }
            else
            {
                query = query.Where(l => l.DataVencimento >= inicio && l.DataVencimento <= fim);
            }

            // 2. Lógica de Status (CORRIGIDA)
            if (apenasPagos)
            {
                query = query.Where(l => l.Status == StatusLancamento.Pago);
            }

            if(!incluirCancelados)
            {
                query = query.Where(l => l.Status != StatusLancamento.Cancelado);
            }

            // 3. Outros Filtros
            if (centroCustoId.HasValue && centroCustoId.Value > 0)
            {
                query = query.Where(l => l.CentroCustoId == centroCustoId.Value);
            }

            if (tipo.HasValue)
            {
                query = query.Where(l => l.Tipo == tipo.Value);
            }

            // 4. Ordenação
            // Removemos o OrderBy(Status == Cancelado) intermediário pois ele seria sobrescrito abaixo.
            if (apenasPagos)
                return await query.OrderBy(l => l.DataPagamento).ToListAsync();
            else
                return await query.OrderBy(l => l.DataVencimento).ToListAsync();
        }

        public async Task<IEnumerable<Lancamento>> GetAllAsync()
        {
            return await _context.Lancamento
                            .AsNoTracking()
                            .Include(l => l.Categoria)
                            .Include(l => l.CentroCusto)
                            .OrderByDescending(l => l.DataVencimento)
                            .ToListAsync();
        }
    }
}