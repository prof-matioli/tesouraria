using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Desktop.Views.Relatorios;
using LiveCharts; // Namespace do LiveCharts
using LiveCharts.Wpf; // Namespace do LiveCharts WPF
using static Tesouraria.Application.DTOs.DashboardResumoDto;
using Tesouraria.Desktop.Views;

namespace Tesouraria.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILancamentoService _lancamentoService;
        private string _saudacaoUsuario;
        public string SaudacaoUsuario
        {
            get => _saudacaoUsuario;
            set => SetProperty(ref _saudacaoUsuario, value);
        }

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
        public ICommand AbrirUsuarioCommand { get; }
        public ICommand CarregarDadosCommand { get; }
        // Sistema
        public ICommand SairCommand { get; }

        // 1. Propriedade para controlar visibilidade de itens de Admin
        private bool _isAdministrador;
        public bool IsAdministrador
        {
            get => _isAdministrador;
            set => SetProperty(ref _isAdministrador, value);
        }

        // Exemplo: Propriedade para itens que só Tesoureiros e Admins veem
        private bool _isTesourariaAcessivel;
        public bool IsTesourariaAcessivel
        {
            get => _isTesourariaAcessivel;
            set => SetProperty(ref _isTesourariaAcessivel, value);
        }

        public MainViewModel(ILancamentoService lancamentoService, IServiceProvider serviceProvider)
        {
            _lancamentoService = lancamentoService;
            _serviceProvider = serviceProvider;

            _resumo = new DashboardResumoDto();
            LabelsGrafico = [];

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
            AbrirUsuarioCommand = new RelayCommand(_ => AbrirJanela<CadastroUsuarioWindow>());

            // 4. Sair
            SairCommand = new RelayCommand(_ => FecharJanela<MainWindow>());

            CarregarDadosCommand = new RelayCommand(async _ => await CarregarDashboard());

            // Carrega dados iniciais
            _ = CarregarDashboard();

            // Formatador para eixo Y (Moeda)
            YFormatter = value => value.ToString("C0"); // R$ 1.000

            // Inicializa vazio para não quebrar a View
            SeriesGrafico = new SeriesCollection();

            //CarregarDadosUsuario();

            CarregarPermissoes();
        }

        private void CarregarPermissoes()
        {
            if (SessaoSistema.UsuarioLogado != null)
            {
                // 1. Verificação de Segurança para evitar erro se o Perfil vier nulo
                var nomePerfil = SessaoSistema.UsuarioLogado.Perfil;

                if (string.IsNullOrEmpty(nomePerfil))
                {
                    // Se chegou aqui, o problema é no Repositório (Passo 2)
                    // Vamos forçar false para não quebrar
                    IsAdministrador = false;
                    IsTesourariaAcessivel = false;
                    System.Windows.MessageBox.Show("Erro: O perfil do usuário não foi carregado do banco.");
                    return;
                }

                nomePerfil = nomePerfil.ToLower().Trim(); // Converte para minúsculo e remove espaços

                // 2. Lógica mais flexível (aceita "admin", "administrador", "administrator")
                IsAdministrador = nomePerfil.Contains("admin");

                // Tesouraria: Admin OU Tesoureiro
                IsTesourariaAcessivel = IsAdministrador || nomePerfil.Contains("tesour");

                SaudacaoUsuario = $"Logado como [{SessaoSistema.UsuarioLogado.Nome}]";
            }
            else
            {
                IsAdministrador = false;
                IsTesourariaAcessivel = false;
                SaudacaoUsuario = "Visitante";
            }
        }
        private void CarregarDadosUsuario()
        {
            if (SessaoSistema.UsuarioLogado != null)
            {
                // Pega o nome real do DTO
                SaudacaoUsuario = $"Olá {SessaoSistema.UsuarioLogado.Nome},\n seja bem vindo!";

            }
            else
            {
                // Fallback caso abra a tela sem logar (desenvolvimento)
                SaudacaoUsuario = "Usuário";
            }
        }
        private void FecharJanela<TWindow>() where TWindow : Window
        {
            try
            {
                // Resolve a janela pelo container de DI (garante que o VM dela também seja injetado)
                //var janela = _serviceProvider.GetRequiredService<TWindow>();
                //janela.Close();
                System.Windows.Application.Current.Shutdown();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fechar janela: {ex.Message}\nVerifique se ela foi registrada no App.xaml.cs");
            }

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