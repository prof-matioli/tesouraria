using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Desktop.Services;
using Tesouraria.Application.DTOs;

namespace Tesouraria.Desktop.ViewModels
{
    public class RelatorioViewModel : ViewModelBase
    {
        private readonly ILancamentoService _lancamentoService;
        private readonly IRepository<CentroCusto> _centroCustoRepo;
        private readonly RelatorioPdfService _pdfService;

        // Filtros
        public DateTime DataInicio { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime DataFim { get; set; } = DateTime.Now;

        public ObservableCollection<CentroCusto> CentrosCusto { get; } = new();
        public int? CentroCustoId { get; set; } // Null = Todos

        private bool _filtrarPorPagamento;
        public bool FiltrarPorPagamento
        {
            get => _filtrarPorPagamento;
            set
            {
                _filtrarPorPagamento = value;
                OnPropertyChanged(nameof(FiltrarPorPagamento));
            }
        }

        // Comandos
        public ICommand GerarCommand { get; }

        public RelatorioViewModel(ILancamentoService lancamentoService, 
                                  IRepository<CentroCusto> centroCustoRepo,
                                  RelatorioPdfService pdfService)
        {
            _lancamentoService = lancamentoService;
            _centroCustoRepo = centroCustoRepo;
            _pdfService = pdfService;

            GerarCommand = new RelayCommand(async _ => await GerarRelatorio());
            CarregarListas();
        }

        private async void CarregarListas()
        {
            var custos = await _centroCustoRepo.GetAllAsync();
            CentrosCusto.Add(new CentroCusto { Id = 0, Nome = "TODOS" }); // Opção Padrão
            foreach (var c in custos) CentrosCusto.Add(c);
            CentroCustoId = 0; // Seleciona Todos
            OnPropertyChanged(nameof(CentroCustoId));
        }

        private async Task GerarRelatorio()
        {
            try
            {
                var filtro = new FiltroRelatorioDto
                {
                    DataInicio = DataInicio,
                    DataFim = DataFim,
                    CentroCustoId = CentroCustoId == 0 ? null : CentroCustoId,
                    ApenasPagos = true,
                    IncluirCancelados = false,
                    FiltrarPorDataPagamento = FiltrarPorPagamento
                };

                var dados = await _lancamentoService.GerarRelatorioAsync(filtro);

                if (!dados.Any())
                {
                    MessageBox.Show("Nenhum lançamento encontrado para este período.");
                    return;
                }

                // Gera o PDF
                // Gera o PDF usando a instância injetada, não mais 'new'
                string caminho = Path.Combine(Path.GetTempPath(), $"Relatorio_Caixa_{DateTime.Now:HHmmss}.pdf");

                _pdfService.GerarPdf(dados, filtro, caminho);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar relatório: {ex.Message}");
            }
        }
    }
}