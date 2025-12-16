using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class LancamentoService : ILancamentoService
    {
        // Agora dependemos da abstração, não da implementação concreta do EF
        private readonly ILancamentoRepository _lancamentoRepository;

        // Supondo que você já tenha repositórios genéricos ou específicos para Categoria também
        // Para simplificar o exemplo, focarei no repositório de lançamentos
        private readonly IRepository<CategoriaFinanceira> _categoriaRepository;

        public LancamentoService(
            ILancamentoRepository lancamentoRepository,
            IRepository<CategoriaFinanceira> categoriaRepository)
        {
            _lancamentoRepository = lancamentoRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<IEnumerable<LancamentoDto>> ObterTodosAsync(DateTime inicio, DateTime fim)
        {
            var lancamentos = await _lancamentoRepository.ObterPorPeriodoAsync(inicio, fim);

            return lancamentos.Select(l => new LancamentoDto
            {
                Id = l.Id,
                Descricao = l.Descricao,
                ValorOriginal = l.ValorOriginal,
                ValorPago = l.ValorPago, // Note que na Entidade não é nullable, mas se Status for Pendente, visualmente pode ser null ou 0
                DataVencimento = l.DataVencimento,
                DataPagamento = l.DataPagamento,
                Tipo = l.Tipo,
                Status = l.Status,
                Observacao = l.Observacao,

                // Preenchendo os IDs
                CategoriaId = l.CategoriaId,
                CentroCustoId = l.CentroCustoId,
                FielId = l.FielId,
                FornecedorId = l.FornecedorId,

                // Preenchendo os Nomes
                CategoriaNome = l.Categoria?.Nome ?? "N/A",
                CentroCustoNome = l.CentroCusto?.Nome ?? "N/A",
                PessoaNome = l.Tipo == TipoTransacao.Receita
                    ? (l.Fiel?.Nome ?? "Anônimo")
                    : (l.Fornecedor?.NomeFantasia ?? "Diversos")
            });
        }
        public async Task<int> RegistrarAsync(CriarLancamentoDto dto)
        {
            // Validações de domínio
            var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaId);
            if (categoria == null) throw new Exception("Categoria não encontrada.");

            if (categoria.Tipo != dto.Tipo)
                throw new Exception($"Categoria incompatível com o tipo de lançamento.");

            var lancamento = new Lancamento(
                dto.Descricao,
                dto.Valor,
                dto.DataVencimento,
                dto.Tipo,
                dto.CategoriaId,
                dto.CentroCustoId,
                dto.UsuarioId,
                dto.FielId,
                dto.FornecedorId
            );

            await _lancamentoRepository.AdicionarAsync(lancamento);
            await _lancamentoRepository.CommitAsync(); // Persiste no banco

            return lancamento.Id;
        }

        public async Task BaixarAsync(BaixarLancamentoDto dto)
        {
            var lancamento = await _lancamentoRepository.ObterPorIdAsync(dto.LancamentoId);
            if (lancamento == null) throw new Exception("Lançamento não encontrado.");

            lancamento.Baixar(dto.ValorPago, dto.DataPagamento);

            await _lancamentoRepository.AtualizarAsync(lancamento);
            await _lancamentoRepository.CommitAsync();
        }

        public async Task CancelarAsync(int id)
        {
            var lancamento = await _lancamentoRepository.ObterPorIdAsync(id);
            if (lancamento == null) throw new Exception("Lançamento não encontrado.");

            lancamento.Cancelar();

            await _lancamentoRepository.AtualizarAsync(lancamento);
            await _lancamentoRepository.CommitAsync();
        }

        public async Task<decimal> ObterSaldoPeriodoAsync(DateTime inicio, DateTime fim)
        {
            var receitas = await _lancamentoRepository.ObterTotalPorPeriodoETipoAsync(inicio, fim, TipoTransacao.Receita);
            var despesas = await _lancamentoRepository.ObterTotalPorPeriodoETipoAsync(inicio, fim, TipoTransacao.Despesa);

            return receitas - despesas;
        }

        public async Task<LancamentoDto?> ObterPorIdAsync(int id)
        {
            var l = await _lancamentoRepository.ObterPorIdAsync(id);
            if (l == null) return null;

            return new LancamentoDto
            {
                Id = l.Id,
                Descricao = l.Descricao,
                ValorOriginal = l.ValorOriginal,
                ValorPago = l.ValorPago,
                DataVencimento = l.DataVencimento,
                DataPagamento = l.DataPagamento,
                Tipo = l.Tipo,
                Status = l.Status,
                Observacao = l.Observacao,

                // IDs essenciais para a tela de Edição selecionar os itens corretos
                CategoriaId = l.CategoriaId,
                CentroCustoId = l.CentroCustoId,
                FielId = l.FielId,
                FornecedorId = l.FornecedorId,

                CategoriaNome = l.Categoria?.Nome ?? string.Empty,
                CentroCustoNome = l.CentroCusto?.Nome ?? string.Empty
            };
        }
    }
}