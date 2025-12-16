using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Application.Interfaces
{
    public interface ILancamentoService
    {
        Task<IEnumerable<LancamentoDto>> ObterTodosAsync(DateTime inicio, DateTime fim);
        Task<LancamentoDto?> ObterPorIdAsync(int id);
        Task<int> RegistrarAsync(CriarLancamentoDto dto);
        Task BaixarAsync(BaixarLancamentoDto dto);
        Task CancelarAsync(int id);

        // Útil para Dashboards
        Task<decimal> ObterSaldoPeriodoAsync(DateTime inicio, DateTime fim);
    }
}