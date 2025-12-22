using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Services;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Desktop.Views.Relatorios;
using LiveCharts; // Namespace do LiveCharts
using LiveCharts.Wpf; // Namespace do LiveCharts WPF
using System.Collections.Generic;
using System.Linq;
using static Tesouraria.Application.DTOs.DashboardResumoDto;

namespace Tesouraria.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        public string SaudacaoUsuario { get; set; }
        private readonly ILancamentoService _lancamentoService;

        // --- Propriedades de Dados (Dashboard) ---
        private DashboardResumoDto _resumo;
        public DashboardResumoDto Resumo
        {
            get => _resumo;
            set => SetProperty(ref _resumo, value);
        }

        // --- Propriedades do Gráfico ---
        public SeriesCollection SeriesGrafico { get; set; }
        public string[] LabelsGrafico { get; set; }
        public Func<double, string> YFormatter { get; set; }

        // --- COMANDOS (Ações dos Botões) ---

        // Financeiro
        public ICommand AbrirLancamentosCommand { get; }
        public ICommand AbrirRelatoriosCommand { get; }

        // Cadastros (Estes estavam faltando ou incompletos)
        public ICommand AbrirFieisCommand { get; }
        public ICommand AbrirFornecedoresCommand { get; }
        public ICommand AbrirCentroCustoCommand { get; }
        public ICommand AbrirPlanoContasCommand { get; }
        public ICommand CarregarDadosCommand { get; }
        // Sistema
        public ICommand SairCommand { get; }

        public MainViewModel(ILancamentoService lancamentoService, IServiceProvider serviceProvider)
        {
            _lancamentoService = lancamentoService;
            _serviceProvider = serviceProvider;
            // Define a saudação
            SaudacaoUsuario = $"Bem-vindo, {SessaoSistema.NomeUsuario}";

            // Inicializa DTO vazio para não quebrar a UI antes de carregar
            Resumo = new DashboardResumoDto();

            // --- CONFIGURAÇÃO DAS AÇÕES ---

            AbrirLancamentosCommand = new RelayCommand(_ => AbrirJanela<LancamentoListaView>());
            AbrirRelatoriosCommand = new RelayCommand(_ => AbrirJanela<RelatorioView>());

            // Abre a lista de Fiéis
            AbrirFieisCommand = new RelayCommand(_ => AbrirJanela<CadastroFielWindow>());            // Abre a lista de Fornecedores
            AbrirFornecedoresCommand = new RelayCommand(_ => AbrirJanela<CadastroFornecedorWindow>());
            // Abre a lista de Centros de Custo
            AbrirCentroCustoCommand = new RelayCommand(_ => AbrirJanela<CadastroCentroCustoWindow>());
            // Abre a lista de Categorias (Plano de Contas)
            AbrirPlanoContasCommand = new RelayCommand(_ => AbrirJanela<CadastroCategoriaFinanceiraWindow>());

            // 4. Sair
            SairCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());

            CarregarDadosCommand = new RelayCommand(async _ => await CarregarDashboard());

            // Carrega dados iniciais
            _ = CarregarDashboard();

            // Formatador para eixo Y (Moeda)
            YFormatter = value => value.ToString("C0"); // R$ 1.000

            // Inicializa vazio para não quebrar a View
            SeriesGrafico = new SeriesCollection();
        }

        private async Task CarregarDashboard()
        {
            try
            {
                Resumo = await _lancamentoService.ObterResumoDashboardAsync();
                // --- Configura o Gráfico com os dados retornados ---
                ConfigurarGrafico(Resumo.Historico);
            }
            catch (Exception ex)
            {
                // Em produção, logar o erro
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dashboard: {ex.Message}");
            }
        }

        private void ConfigurarGrafico(List<GraficoPontoDto> dados)
        {
            // Limpa e recria as séries
            SeriesGrafico.Clear();

            // Série de Receitas (Verde)
            SeriesGrafico.Add(new LineSeries
            {
                Title = "Receitas",
                Values = new ChartValues<decimal>(dados.Select(x => x.Receitas)),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                Stroke = System.Windows.Media.Brushes.SeaGreen,
                Fill = System.Windows.Media.Brushes.Transparent // Sem preenchimento embaixo da linha
            });

            // Série de Despesas (Vermelho)
            SeriesGrafico.Add(new LineSeries
            {
                Title = "Despesas",
                Values = new ChartValues<decimal>(dados.Select(x => x.Despesas)),
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                Stroke = System.Windows.Media.Brushes.IndianRed,
                Fill = System.Windows.Media.Brushes.Transparent
            });

            // Configura o eixo X (Meses)
            LabelsGrafico = dados.Select(x => x.Mes).ToArray();
            OnPropertyChanged(nameof(LabelsGrafico)); // Notifica a View que os labels mudaram
        }
        // Método genérico para abrir janelas usando DI
        private void AbrirJanela<TWindow>() where TWindow : Window
        {
            try
            {
                // Resolve a janela pelo container de DI (garante que o VM dela também seja injetado)
                var janela = _serviceProvider.GetRequiredService<TWindow>();
                janela.Show(); 

                // Ao fechar a janela filha, recarrega o dashboard para atualizar saldos
                _ = CarregarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela: {ex.Message}\nVerifique se ela foi registrada no App.xaml.cs");
            }
        }

        // Método genérico para abrir janelas modais (bloqueia a de trás)
        private void AbrirJanelaModal<T>() where T : Window
        {
            try
            {
                var view = _serviceProvider.GetRequiredService<T>();
                view.Owner = System.Windows.Application.Current.MainWindow;
                view.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Exibe a mensagem de confirmação
            var result = MessageBox.Show("Deseja realmente sair do sistema?",
                                         "Confirmação",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            // Se o usuário clicar em 'Não', cancela o fechamento
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }

            // Se clicar em 'Sim', o evento prossegue e a janela fecha.
        }

    }
}