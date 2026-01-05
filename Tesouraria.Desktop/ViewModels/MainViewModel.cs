using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Tesouraria.Application.Services;
using Tesouraria.Desktop.Core;

namespace Tesouraria.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        // --- NAVEGAÇÃO SPA ---

        // Esta propriedade define o que aparece no centro da tela (Dashboard, Lista de Fiéis, etc)
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // --- Propriedades Globais (Shell) ---
        private string _saudacaoUsuario;
        public string SaudacaoUsuario
        {
            get => _saudacaoUsuario;
            set => SetProperty(ref _saudacaoUsuario, value);
        }

        private bool _isAdministrador;
        public bool IsAdministrador
        {
            get => _isAdministrador;
            set => SetProperty(ref _isAdministrador, value);
        }

        private bool _isTesourariaAcessivel;
        public bool IsTesourariaAcessivel
        {
            get => _isTesourariaAcessivel;
            set => SetProperty(ref _isTesourariaAcessivel, value);
        }

        // --- COMANDOS DE NAVEGAÇÃO ---
        public ICommand NavegarHomeCommand { get; }

        // Financeiro
        public ICommand NavegarLancamentosCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }

        // Cadastros
        public ICommand NavegarFieisCommand { get; }
        public ICommand NavegarFornecedoresCommand { get; }
        public ICommand NavegarCentroCustoCommand { get; }
        public ICommand NavegarPlanoContasCommand { get; }
        public ICommand NavegarUsuarioCommand { get; }

        public ICommand FazerBackupCommand { get; }
        public ICommand NavegarImportacaoCommand { get; }

        // Sistema
        //public ICommand SairCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // --- Configuração dos Comandos de Navegação ---

            // Dashboard (Home)
            NavegarHomeCommand = new RelayCommand(_ => NavegarPara<DashboardViewModel>());

            // Financeiro
            NavegarLancamentosCommand = new RelayCommand(_ => NavegarPara<LancamentoListaViewModel>());
            NavegarRelatoriosCommand = new RelayCommand(_ => NavegarPara<RelatorioViewModel>());

            // Cadastros
            NavegarFieisCommand = new RelayCommand(_ => NavegarPara<FielListaViewModel>());
            NavegarFornecedoresCommand = new RelayCommand(_ => NavegarPara<FornecedorListaViewModel>());
            NavegarCentroCustoCommand = new RelayCommand(_ => NavegarPara<CentroCustoListaViewModel>());
            NavegarPlanoContasCommand = new RelayCommand(_ => NavegarPara<CategoriaFinanceiraListaViewModel>());
            NavegarUsuarioCommand = new RelayCommand(_ => NavegarPara<UsuarioListaViewModel>()); // Assumindo lista, não cadastro direto

            //Backup
            FazerBackupCommand = new RelayCommand(_ => FazerBackup());
            
            //Importação de extrato bancário
            NavegarImportacaoCommand = new RelayCommand(_ => NavegarPara<ImportacaoExtratoViewModel>());

            // Sair
            //SairCommand = new RelayCommand(_ => FecharSistema());

            // Carregamentos Iniciais
            CarregarPermissoes();

            // Inicia o sistema já no Dashboard
            NavegarPara<DashboardViewModel>();
        }

        private void FazerBackup()
        {
            // Instancia o serviço (ou injeta via Dependecy Injection se preferir)
            var backupService = new BackupService();
            backupService.RealizarBackup();
        }

        // --- LÓGICA DE NAVEGAÇÃO (SPA) ---
        private void NavegarPara<TViewModel>() where TViewModel : ViewModelBase
        {
            try
            {
                // Resolve a ViewModel solicitada via Injeção de Dependência
                var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

                // Define como a tela atual. O ContentControl na MainWindow vai renderizar a View correspondente.
                CurrentViewModel = viewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar: {ex.Message}\nVerifique se {typeof(TViewModel).Name} foi registrado no App.xaml.cs");
            }
        }

        private void CarregarPermissoes()
        {
            if (SessaoSistema.UsuarioLogado != null)
            {
                var nomePerfil = SessaoSistema.UsuarioLogado.Perfil;

                if (string.IsNullOrEmpty(nomePerfil))
                {
                    IsAdministrador = false;
                    IsTesourariaAcessivel = false;
                    return;
                }

                nomePerfil = nomePerfil.ToLower().Trim();
                IsAdministrador = nomePerfil.Contains("admin");
                IsTesourariaAcessivel = IsAdministrador || nomePerfil.Contains("tesour");

                SaudacaoUsuario = $"Logado como: {SessaoSistema.UsuarioLogado.Nome}";
            }
            else
            {
                IsAdministrador = false;
                IsTesourariaAcessivel = false;
                SaudacaoUsuario = "Modo Visitante / Debug";
            }
        }

        /*
        private void FecharSistema()
        {
            var result = MessageBox.Show("Deseja realmente sair do sistema?", "Sair", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
        */
    }
}