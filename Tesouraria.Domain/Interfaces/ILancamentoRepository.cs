using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Domain.Interfaces
{
    public interface ILancamentoRepository
    {
        Task<Lancamento?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Lancamento lancamento);
        Task AtualizarAsync(Lancamento lancamento);

        // Métodos específicos para consultas financeiras
        Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
        Task<decimal> ObterTotalPorPeriodoETipoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo);
        Task<decimal> ObterTotalPrevistoAsync(DateTime inicio, DateTime fim, TipoTransacao tipo);
        // Commit (caso não esteja usando UnitOfWork separado)
        Task<bool> CommitAsync();
    }
}