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

        public async Task<IEnumerable<Lancamento>> ObterFiltradosAsync(
                                    DateTime inicio,
                                    DateTime fim,
                                    int? centroCustoId,
                                    TipoTransacao? tipo,
                                    bool apenasPagos)
        {
            var query = _context.Lancamento
                .AsNoTracking() // Importante para performance de relatório
                .Include(l => l.Categoria)
                .Include(l => l.CentroCusto)
                .Include(l => l.Fiel)
                .Include(l => l.Fornecedor)
                .AsQueryable();

            // Filtro de Datas e Status
            if (apenasPagos)
            {
                query = query.Where(l => l.Status == StatusLancamento.Pago
                                         && l.DataPagamento >= inicio
                                         && l.DataPagamento <= fim);
            }
            else
            {
                query = query.Where(l => l.Status != StatusLancamento.Cancelado
                                         && l.DataVencimento >= inicio
                                         && l.DataVencimento <= fim);
            }

            // Filtro de Centro de Custo
            if (centroCustoId.HasValue && centroCustoId.Value > 0)
            {
                query = query.Where(l => l.CentroCustoId == centroCustoId.Value);
            }

            // Filtro de Tipo (Receita/Despesa)
            if (tipo.HasValue)
            {
                query = query.Where(l => l.Tipo == tipo.Value);
            }

            // Ordenação
            if (apenasPagos)
                return await query.OrderBy(l => l.DataPagamento).ToListAsync();
            else
                return await query.OrderBy(l => l.DataVencimento).ToListAsync();
        }

        public async Task<IEnumerable<Lancamento>> GetAllAsync()
        {
            return await _context.Lancamento
                            .AsNoTracking() // Melhora performance pois não rastreia alterações (apenas leitura)

                            // === EAGER LOADING (Carregamento dos Relacionamentos) ===
                            // Isso preenche os objetos dentro do Lançamento para exibir os Nomes na tela
                            .Include(l => l.Categoria)
                            .Include(l => l.CentroCusto)
                            // .Include(l => l.Fornecedor) // Descomente se tiver relacionamento direto
                            // .Include(l => l.Fiel)       // Descomente se tiver relacionamento direto

                            .OrderByDescending(l => l.DataVencimento) // Ordena por data (mais recentes primeiro)
                            .ToListAsync();
        }

    }
}