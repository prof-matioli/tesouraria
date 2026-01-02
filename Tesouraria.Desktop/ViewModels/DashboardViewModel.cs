using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; // Necessário para MessageBox
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using static Tesouraria.Application.DTOs.DashboardResumoDto;

namespace Tesouraria.Desktop.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly ILancamentoService _lancamentoService;

        private DashboardResumoDto _resumo;
        public DashboardResumoDto Resumo
        {
            get => _resumo;
            set => SetProperty(ref _resumo, value);
        }

        // Propriedades do Gráfico
        private SeriesCollection _seriesGrafico;
        public SeriesCollection SeriesGrafico
        {
            get => _seriesGrafico;
            set => SetProperty(ref _seriesGrafico, value);
        }

        private string[] _labelsGrafico;
        public string[] LabelsGrafico
        {
            get => _labelsGrafico;
            set => SetProperty(ref _labelsGrafico, value);
        }

        public Func<double, string> YFormatter { get; set; }

        public DashboardViewModel(ILancamentoService lancamentoService)
        {
            _lancamentoService = lancamentoService;

            // Inicialização padrão para evitar NullReference na View
            Resumo = new DashboardResumoDto();
            SeriesGrafico = new SeriesCollection();
            LabelsGrafico = new string[] { };
            YFormatter = value => value.ToString("C0"); // Formatação em Moeda

            // Inicia o carregamento
            _ = CarregarDashboard();
        }

        private async Task CarregarDashboard()
        {
            try
            {
                Resumo = await _lancamentoService.ObterResumoDashboardAsync();

                // Verifica se vieram dados para o gráfico
                if (Resumo?.Historico != null && Resumo.Historico.Any())
                {
                }
                    ConfigurarGrafico(Resumo.Historico);
            }
            catch (Exception ex)
            {
                // CORREÇÃO: Exibe o erro para não falhar silenciosamente
                MessageBox.Show($"Erro ao carregar Dashboard: {ex.Message}");
            }
        }

        private void ConfigurarGrafico(List<GraficoPontoDto> dados)
        {
            // Limpa séries anteriores para não duplicar se recarregar
            SeriesGrafico.Clear();

            // CORREÇÃO: LiveCharts prefere 'double' para renderização.
            // Convertemos os valores de decimal para double.
            var valoresReceita = new ChartValues<double>(dados.Select(x => (double)x.Receitas));
            var valoresDespesa = new ChartValues<double>(dados.Select(x => (double)x.Despesas));

            SeriesGrafico.Add(new LineSeries
            {
                Title = "Receitas",
                Values = valoresReceita,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                Stroke = System.Windows.Media.Brushes.SeaGreen,
                Fill = System.Windows.Media.Brushes.Transparent // Área transparente (apenas linha)
            });

            SeriesGrafico.Add(new LineSeries
            {
                Title = "Despesas",
                Values = valoresDespesa,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                Stroke = System.Windows.Media.Brushes.IndianRed,
                Fill = System.Windows.Media.Brushes.Transparent
            });

            // Configura os Labels (Eixo X - Meses)
            LabelsGrafico = dados.Select(x => x.Mes).ToArray();
        }
    }
}