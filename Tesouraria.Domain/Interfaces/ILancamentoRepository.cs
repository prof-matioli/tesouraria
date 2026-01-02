using Tesouraria.Domain.Entities;

namespace Tesouraria.Domain.Interfaces
{
    public interface ILancamentoRepository 
    {
        Task<Lancamento?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Lancamento lancamento);
        Task AtualizarAsync(Lancamento lancamento);

        // Métodos específicos para consultas financeiras
        Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, bool incluirCancelados);
        Task<decimal> ObterTotalPorPeriodoETipoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo);
        Task<decimal> ObterTotalPrevistoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo);
        // Commit (caso não esteja usando UnitOfWork separado)
        Task<bool> CommitAsync();

        Task<IEnumerable<Lancamento>> ObterFiltradosAsync(
            DateTime inicio,
            DateTime fim,
            int? centroCustoId,
            TipoTransacao? tipo,
            bool apenasPagos,
            bool incluirCancelados,
            bool filtrarPorPagamento);

        // Método específico para trazer dados completos (com Includes)
        new Task<IEnumerable<Lancamento>> GetAllAsync();
    }
}