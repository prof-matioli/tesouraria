using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;
using static Tesouraria.Application.DTOs.DashboardResumoDto;

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

        public async Task<IEnumerable<LancamentoDto>> GerarRelatorioAsync(FiltroRelatorioDto filtro)
        {
            // Chama o método novo do repositório
            var lista = await _lancamentoRepository.ObterFiltradosAsync(
                filtro.DataInicio,
                filtro.DataFim,
                filtro.CentroCustoId,
                filtro.Tipo,
                filtro.ApenasPagos,
                filtro.IncluirCancelados,
                filtro.FiltrarPorDataPagamento
            );

            // Faz o Mapeamento para DTO
            return lista.Select(l => new LancamentoDto
            {
                Id = l.Id,
                Descricao = l.Descricao,
                ValorOriginal = l.ValorOriginal,
                ValorPago = l.ValorPago,
                DataVencimento = l.DataVencimento,
                DataPagamento = l.DataPagamento,
                Tipo = l.Tipo,
                Status = l.Status,
                CategoriaNome = l.Categoria?.Nome ?? "",
                CentroCustoNome = l.CentroCusto?.Nome ?? "",
                PessoaNome = l.Tipo == TipoTransacao.Receita ? (l.Fiel?.Nome) : (l.Fornecedor?.NomeFantasia)
            });
        }

        public async Task<decimal> ObterSaldoPrevistoAsync(DateTime inicio, DateTime fim)
        {
            var receitas = await _lancamentoRepository.ObterTotalPrevistoAsync(inicio, fim, TipoTransacao.Receita);
            var despesas = await _lancamentoRepository.ObterTotalPrevistoAsync(inicio, fim, TipoTransacao.Despesa);

            return receitas - despesas;
        }

        public async Task<IEnumerable<LancamentoDto>> ObterTodosAsync(DateTime inicio, DateTime fim, bool incluirCancelados)
        {
            var lancamentos = await _lancamentoRepository.ObterPorPeriodoAsync(inicio, fim, incluirCancelados);

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
                    : (l.Fornecedor?.RazaoSocial ?? "Diversos")
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

        public async Task AtualizarAsync(int id, CriarLancamentoDto dto)
        {
            var lancamento = await _lancamentoRepository.ObterPorIdAsync(id);
            if (lancamento == null) throw new Exception("Lançamento não encontrado.");

            // Validação de categoria (mesma lógica da criação)
            var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaId); // Supondo que você injetou este repo
            if (categoria != null && categoria.Tipo != dto.Tipo)
                throw new Exception("O tipo da Categoria não corresponde ao tipo do Lançamento.");

            // Chama o método do domínio
            lancamento.AtualizarDados(
                dto.Descricao,
                dto.Valor,
                dto.DataVencimento,
                dto.Tipo,
                dto.CategoriaId,
                dto.CentroCustoId,
                dto.FielId,
                dto.FornecedorId,
                dto.Observacao
            );

            await _lancamentoRepository.AtualizarAsync(lancamento);
            await _lancamentoRepository.CommitAsync();
        }

        public async Task EstornarLancamento(int id)
        {
            var lancamento = await _lancamentoRepository.ObterPorIdAsync(id);
            if (lancamento == null) throw new Exception("Lançamento não encontrado.");

            lancamento.EstornarBaixa();

            await _lancamentoRepository.AtualizarAsync(lancamento);
            await _lancamentoRepository.CommitAsync();
        }

        public async Task<DashboardResumoDto> ObterResumoDashboardAsync()
        {
            // Busca todos os lançamentos (Idealmente, filtrar por mês/ano no Repositório para performance)
            // Aqui faremos uma implementação simplificada buscando tudo e filtrando em memória
            // Em produção: _repository.GetByDateRange(...)

            var todosLancamentos = await _lancamentoRepository.GetAllAsync();

            // Filtra mês atual
            var hoje = DateTime.Now;
            var lancamentosMes = todosLancamentos
                .Where(x => x.DataVencimento.Month == hoje.Month && x.DataVencimento.Year == hoje.Year)
                .ToList();

            var resumo = new DashboardResumoDto
            {
                TotalReceitas = lancamentosMes
                    .Where(x => x.Tipo == TipoTransacao.Receita)
                    .Sum(x => x.ValorPago),

                TotalDespesas = lancamentosMes
                    .Where(x => x.Tipo == TipoTransacao.Despesa)
                    .Sum(x => x.ValorPago),

                ComparativoReceita = "+ 0% vs mês ant.", // Lógica de comparação viria aqui
                ComparativoDespesa = "Dentro da meta"
            };

            // 2. Lógica do Gráfico (Últimos 6 meses)
            var dataInicioGrafico = hoje.AddMonths(-5); // 5 meses atrás + atual = 6
            var dataCorte = new DateTime(dataInicioGrafico.Year, dataInicioGrafico.Month, 1); // Dia 1

            var dadosHistoricos = todosLancamentos
                .Where(x => x.DataVencimento >= dataCorte)
                .GroupBy(x => new { x.DataVencimento.Year, x.DataVencimento.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new GraficoPontoDto
                {
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM").ToUpper(), // "JAN"
                    Receitas = g.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago),
                    Despesas = g.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago)
                })
                .ToList();
            return resumo;
        }
    }
}