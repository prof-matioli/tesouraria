using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;

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

        // --- FILTROS ---
        private DateTime _filtroDataInicio;
        public DateTime FiltroDataInicio { get => _filtroDataInicio; set => SetProperty(ref _filtroDataInicio, value); }

        private DateTime _filtroDataFim;
        public DateTime FiltroDataFim { get => _filtroDataFim; set => SetProperty(ref _filtroDataFim, value); }

        private int? _filtroCentroCustoId;
        public int? FiltroCentroCustoId { get => _filtroCentroCustoId; set => SetProperty(ref _filtroCentroCustoId, value); }

        private bool _filtroApenasPagos;
        public bool FiltroApenasPagos { get => _filtroApenasPagos; set => SetProperty(ref _filtroApenasPagos, value); }

        private bool _filtroIncluirCancelados;
        public bool FiltroIncluirCancelados { get => _filtroIncluirCancelados; set => SetProperty(ref _filtroIncluirCancelados, value); }

        // --- COMANDOS ---
        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; } // Usado no botão da linha do grid
        public ICommand BuscarCommand { get; }

        // Comandos Restaurados
        public ICommand BaixarCommand { get; }
        public ICommand EstornarCommand { get; }
        public ICommand CancelarCommand { get; } // Botão do rodapé

        public LancamentoListaViewModel(
            ILancamentoService lancamentoService,
            IRepository<CentroCusto> centroCustoRepository,
            IServiceProvider serviceProvider)
        {
            _lancamentoService = lancamentoService;
            _centroCustoRepository = centroCustoRepository;
            _serviceProvider = serviceProvider;

            // Configuração Inicial dos Filtros
            var hoje = DateTime.Now;
            FiltroDataInicio = new DateTime(hoje.Year, hoje.Month, 1);
            FiltroDataFim = FiltroDataInicio.AddMonths(1).AddDays(-1);
            FiltroApenasPagos = false;
            FiltroIncluirCancelados = false;

            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));
            BuscarCommand = new RelayCommand(async _ => await CarregarDados());

            // Editar: Funciona tanto pelo botão da linha (param int) quanto pelo rodapé (LancamentoSelecionado)
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

            // --- LÓGICA DOS BOTÕES RESTAURADOS ---

            // BAIXAR: Habilitado apenas se selecionado e estiver Pendente
            BaixarCommand = new RelayCommand(async _ => await BaixarLancamento(),
                _ => LancamentoSelecionado != null && LancamentoSelecionado.Status == StatusLancamento.Pendente);

            // ESTORNAR: Habilitado apenas se selecionado e estiver Pago
            EstornarCommand = new RelayCommand(async _ => await EstornarLancamento(),
                _ => LancamentoSelecionado != null && (LancamentoSelecionado.Status == StatusLancamento.Pago));

            // CANCELAR (Rodapé): Habilitado se selecionado e não estiver cancelado
            CancelarCommand = new RelayCommand(async _ => await Cancelar(LancamentoSelecionado!.Id),
                _ => LancamentoSelecionado != null && LancamentoSelecionado.Status != StatusLancamento.Cancelado);

            _ = CarregarListasAuxiliares();
            _ = CarregarDados();
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

                        // Se já tem valor pago, soma no realizado. Se não tem mas está "Pago", usa o original.
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
                // 1. Obtém a nova janela via injeção de dependência
                var formView = _serviceProvider.GetRequiredService<LancamentoCadastroView>();

                // 2. CORREÇÃO DO ERRO DE OWNER:
                // Verifica explicitamente se a MainWindow existe E se ela não é a própria janela que estamos abrindo.
                var mainWindow = System.Windows.Application.Current.MainWindow;

                if (mainWindow != null && mainWindow != formView)
                {
                    formView.Owner = mainWindow;
                }
                else
                {
                    // Se não puder definir o Owner (ex: MainWindow nula), 
                    // garantimos que ela abra centralizada na tela
                    formView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                // 3. Configura a ViewModel e abre a janela
                var vm = formView.DataContext as LancamentoCadastroViewModel;
                if (vm != null)
                {
                    // Obs: A assinatura do evento RequestClose já está no construtor da View (code-behind),
                    // então não precisamos fazer "vm.RequestClose += ..." aqui.

                    _ = vm.Carregar(id);
                    formView.ShowDialog(); // Abre como Modal
                    _ = CarregarDados();   // Atualiza a lista ao fechar
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