using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Desktop.Properties;

namespace Tesouraria.Desktop.ViewModels
{
    public class LancamentoListaViewModel : ViewModelBase
    {
        private readonly ILancamentoService _lancamentoService;
        private readonly IRepository<CentroCusto> _centroCustoRepository;
        private readonly IServiceProvider _serviceProvider;

        // --- LISTAS ---
        public ObservableCollection<LancamentoDto> ListaLancamentos { get; } = new();
        public ObservableCollection<CentroCusto> CentrosCusto { get; } = new();

        // --- PROPRIEDADES DE FILTRO COM PERSISTÊNCIA ---

        private DateTime _filtroDataInicio;
        public DateTime FiltroDataInicio
        {
            get => _filtroDataInicio;
            set
            {
                if (SetProperty(ref _filtroDataInicio, value))
                {
                    // 1. Salva a preferência assim que o usuário muda
                    Settings.Default.FiltroDataInicio = value;
                    Settings.Default.Save();

                    // Opcional: Recarregar a grid automaticamente ao mudar a data
                    // _ = CarregarDados(); 
                }
            }
        }

        private DateTime _filtroDataFim;
        public DateTime FiltroDataFim
        {
            get => _filtroDataFim;
            set
            {
                if (SetProperty(ref _filtroDataFim, value))
                {
                    // 1. Salva a preferência
                    Settings.Default.FiltroDataFim = value;
                    Settings.Default.Save();

                    // Opcional: Recarregar a grid automaticamente ao mudar a data
                    _ = CarregarDados(); 
                }
            }
        }


        // --- SELEÇÃO (Para os botões do rodapé funcionarem) ---
        private LancamentoDto? _lancamentoSelecionado;
        public LancamentoDto? LancamentoSelecionado
        {
            get => _lancamentoSelecionado;
            set
            {
                SetProperty(ref _lancamentoSelecionado, value);
                // Notifica a interface para habilitar/desabilitar os botões
                (BaixarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EstornarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- TOTAIS FINANCEIROS ---
        private decimal _saldoPrevisto;
        public decimal SaldoPrevisto
        {
            get => _saldoPrevisto;
            set => SetProperty(ref _saldoPrevisto, value);
        }

        private decimal _saldoRealizado;
        public decimal SaldoRealizado
        {
            get => _saldoRealizado;
            set => SetProperty(ref _saldoRealizado, value);
        }

        // --- OUTROS FILTROS ---
        private int? _filtroCentroCustoId;
        public int? FiltroCentroCustoId { get => _filtroCentroCustoId; set => SetProperty(ref _filtroCentroCustoId, value); }
        /*
                private bool _filtroApenasPagos;
                public bool FiltroApenasPagos { get => _filtroApenasPagos; set => SetProperty(ref _filtroApenasPagos, value); }

                private bool _filtroIncluirCancelados;
                public bool FiltroIncluirCancelados { get => _filtroIncluirCancelados; set => SetProperty(ref _filtroIncluirCancelados, value); }
        */

        private bool _filtroApenasPagos;
        public bool FiltroApenasPagos
        {
            get => _filtroApenasPagos;
            set
            {
                if (SetProperty(ref _filtroApenasPagos, value))
                {
                    // Assim que marcar/desmarcar, recarrega a lista
                    _ = CarregarDados();
                }
            }
        }

        private bool _filtroIncluirCancelados;
        public bool FiltroIncluirCancelados
        {
            get => _filtroIncluirCancelados;
            set
            {
                if (SetProperty(ref _filtroIncluirCancelados, value))
                {
                    // Assim que marcar/desmarcar, recarrega a lista
                    _ = CarregarDados();
                }
            }
        }


        // --- COMANDOS ---
        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }
        public ICommand BaixarCommand { get; }
        public ICommand EstornarCommand { get; }
        public ICommand CancelarCommand { get; }

        public LancamentoListaViewModel(
            ILancamentoService lancamentoService,
            IRepository<CentroCusto> centroCustoRepository,
            IServiceProvider serviceProvider)
        {
            _lancamentoService = lancamentoService;
            _centroCustoRepository = centroCustoRepository;
            _serviceProvider = serviceProvider;

            // 1. Carrega as datas salvas (ou define o padrão se for a 1ª vez)
            InicializarDatas();

            // Configuração Inicial dos Filtros Booleanos
            FiltroApenasPagos = false;
            FiltroIncluirCancelados = false;

            // --- Inicialização dos Comandos ---
            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));
            BuscarCommand = new RelayCommand(async _ => await CarregarDados());

            // Editar
            EditarCommand = new RelayCommand(param =>
            {
                if (param is int id) AbrirFormulario(id);
                else if (LancamentoSelecionado != null) AbrirFormulario(LancamentoSelecionado.Id);
            });

            // Excluir (Lixeira da linha)
            ExcluirCommand = new RelayCommand(async param =>
            {
                if (param is int id) await Cancelar(id);
            });

            // Baixar
            BaixarCommand = new RelayCommand(async _ => await BaixarLancamento(),
                _ => LancamentoSelecionado != null && LancamentoSelecionado.Status == StatusLancamento.Pendente);

            // Estornar
            EstornarCommand = new RelayCommand(async _ => await EstornarLancamento(),
                _ => LancamentoSelecionado != null && LancamentoSelecionado.Status == StatusLancamento.Pago);

            // Cancelar (Rodapé)
            CancelarCommand = new RelayCommand(async _ => await Cancelar(LancamentoSelecionado!.Id),
                _ => LancamentoSelecionado != null && LancamentoSelecionado.Status != StatusLancamento.Cancelado);

            // Carregamentos Iniciais
            _ = CarregarListasAuxiliares();
            _ = CarregarDados();
        }

        private void InicializarDatas()
        {
            try
            {
                var inicioSalvo = Settings.Default.FiltroDataInicio;
                var fimSalvo = Settings.Default.FiltroDataFim;

                // Validação: Se o ano for 1 (DateTime.MinValue), considera que nunca foi salvo
                bool nuncaSalvou = inicioSalvo.Year < 2000;

                if (nuncaSalvou)
                {
                    // Padrão: Mês Atual
                    var hoje = DateTime.Today;
                    // IMPORTANTE: Definimos direto nas variáveis privadas para não disparar o Save() na inicialização
                    _filtroDataInicio = new DateTime(hoje.Year, hoje.Month, 1);
                    _filtroDataFim = _filtroDataInicio.AddMonths(1).AddDays(-1);
                }
                else
                {
                    // Recupera o que estava salvo
                    _filtroDataInicio = inicioSalvo;
                    _filtroDataFim = fimSalvo;
                }

                // Notifica a tela que os valores mudaram
                OnPropertyChanged(nameof(FiltroDataInicio));
                OnPropertyChanged(nameof(FiltroDataFim));
            }
            catch
            {
                // Fallback de segurança
                var hoje = DateTime.Today;
                _filtroDataInicio = new DateTime(hoje.Year, hoje.Month, 1);
                _filtroDataFim = _filtroDataInicio.AddMonths(1).AddDays(-1);
            }
        }

        private async Task CarregarListasAuxiliares()
        {
            try
            {
                var custos = await _centroCustoRepository.GetAllAsync();
                CentrosCusto.Clear();
                CentrosCusto.Add(new CentroCusto { Id = 0, Nome = "TODOS" });
                foreach (var c in custos.OrderBy(x => x.Nome)) CentrosCusto.Add(c);
                FiltroCentroCustoId = 0;
            }
            catch { }
        }

        private async Task CarregarDados()
        {
            try
            {
                ListaLancamentos.Clear();

                var filtroDto = new FiltroRelatorioDto
                {
                    DataInicio = FiltroDataInicio,
                    DataFim = FiltroDataFim,
                    CentroCustoId = (FiltroCentroCustoId == 0) ? null : FiltroCentroCustoId,
                    ApenasPagos = FiltroApenasPagos,
                    IncluirCancelados = FiltroIncluirCancelados
                };

                var dados = await _lancamentoService.GerarRelatorioAsync(filtroDto);

                decimal receitaPrev = 0, despesaPrev = 0;
                decimal receitaReal = 0, despesaReal = 0;

                foreach (var item in dados)
                {
                    ListaLancamentos.Add(item);

                    // Cálculo dos Totais (Ignora Cancelados)
                    if (item.Status != StatusLancamento.Cancelado)
                    {
                        if (item.Tipo == TipoTransacao.Receita) receitaPrev += item.ValorOriginal;
                        else despesaPrev += item.ValorOriginal;

                        // Lógica de saldo realizado
                        var valorBaixado = item.ValorPago ?? (item.Status == StatusLancamento.Pago ? item.ValorOriginal : 0);

                        if (item.Tipo == TipoTransacao.Receita) receitaReal += valorBaixado;
                        else despesaReal += valorBaixado;
                    }
                }

                SaldoPrevisto = receitaPrev - despesaPrev;
                SaldoRealizado = receitaReal - despesaReal;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar lançamentos: {ex.Message}");
            }
        }

        private void AbrirFormulario(int id)
        {
            try
            {
                var formView = _serviceProvider.GetRequiredService<LancamentoCadastroView>();
                var mainWindow = System.Windows.Application.Current.MainWindow;

                if (mainWindow != null && mainWindow != formView)
                {
                    formView.Owner = mainWindow;
                }
                else
                {
                    formView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                var vm = formView.DataContext as LancamentoCadastroViewModel;
                if (vm != null)
                {
                    _ = vm.Carregar(id);
                    formView.ShowDialog();
                    _ = CarregarDados();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir formulário: {ex.Message}");
            }
        }

        // --- OPERAÇÕES ---

        private async Task BaixarLancamento()
        {
            if (LancamentoSelecionado == null) return;

            if (MessageBox.Show($"Confirma a baixa de '{LancamentoSelecionado.Descricao}' no valor de {LancamentoSelecionado.ValorOriginal:C2}?",
                "Baixar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var baixaDto = new BaixarLancamentoDto
                    {
                        LancamentoId = LancamentoSelecionado.Id,
                        DataPagamento = DateTime.Now,
                        ValorPago = LancamentoSelecionado.ValorOriginal
                    };
                    await _lancamentoService.BaixarAsync(baixaDto);
                    await CarregarDados();
                }
                catch (Exception ex) { MessageBox.Show($"Erro ao baixar: {ex.Message}"); }
            }
        }

        private async Task EstornarLancamento()
        {
            if (LancamentoSelecionado == null) return;
            if (MessageBox.Show($"Deseja estornar '{LancamentoSelecionado.Descricao}'?", "Estornar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _lancamentoService.EstornarLancamento(LancamentoSelecionado.Id);
                    await CarregarDados();
                }
                catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}"); }
            }
        }

        private async Task Cancelar(int id)
        {
            if (MessageBox.Show("Deseja realmente CANCELAR este lançamento? Irreversível.", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _lancamentoService.CancelarAsync(id);
                    await CarregarDados();
                }
                catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}"); }
            }
        }
    }
}