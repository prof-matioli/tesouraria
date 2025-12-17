using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection; // <--- NECESSÁRIO PARA O SERVICE PROVIDER
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Desktop.ViewModels
{
    public class LancamentoListaViewModel : ViewModelBase
    {
        private readonly ILancamentoService _lancamentoService;
        private readonly IServiceProvider _serviceProvider; // <--- NOVA DEPENDÊNCIA
        
        // Comandos
        public ICommand BuscarCommand { get; }
        public ICommand NovoCommand { get; }
        public ICommand BaixarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EstornarCommand { get; }
        private decimal _saldoPrevisto;
        public decimal SaldoPrevisto
        {
            get => _saldoPrevisto;
            set => SetProperty(ref _saldoPrevisto, value);
        }

        // Filtros
        private DateTime _dataInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime DataInicio
        {
            get => _dataInicio;
            set { SetProperty(ref _dataInicio, value); }
        }

        private DateTime _dataFim = DateTime.Now;
        public DateTime DataFim
        {
            get => _dataFim;
            set { SetProperty(ref _dataFim, value); }
        }

        // Listagem
        public ObservableCollection<LancamentoDto> Lancamentos { get; } = new();

        private LancamentoDto? _lancamentoSelecionado;

        public LancamentoDto? LancamentoSelecionado
        {
            get => _lancamentoSelecionado;
            set
            {
                SetProperty(ref _lancamentoSelecionado, value);
                (BaixarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EstornarCommand as RelayCommand)?.RaiseCanExecuteChanged(); 
            }
        }

        // Totais
        private decimal _saldoPeriodo;
        public decimal SaldoPeriodo
        {
            get => _saldoPeriodo;
            set => SetProperty(ref _saldoPeriodo, value);
        }


        // CONSTRUTOR
        public LancamentoListaViewModel(
            ILancamentoService lancamentoService,
            IServiceProvider serviceProvider) // <--- INJETANDO O PROVIDER
        {
            _lancamentoService = lancamentoService;
            _serviceProvider = serviceProvider;

            // Define Data Fim para o último dia do mês atual
            DataFim = DataInicio.AddMonths(1).AddDays(-1);

            BuscarCommand = new RelayCommand(async _ => await BuscarAsync());

            // O comando Novo chama o método que usa o Provider
            NovoCommand = new RelayCommand(_ => AbrirNovoLancamento());

            BaixarCommand = new RelayCommand(async _ => await BaixarAsync(), _ => LancamentoSelecionado != null && LancamentoSelecionado.Status == StatusLancamento.Pendente);
            CancelarCommand = new RelayCommand(async _ => await CancelarAsync(), _ => LancamentoSelecionado != null && LancamentoSelecionado.Status != StatusLancamento.Cancelado);

            EditarCommand = new RelayCommand(async _ => await EditarLancamento(), _ => LancamentoSelecionado != null);

            EstornarCommand = new RelayCommand(async _ => await EstornarLancamento(), _ => LancamentoSelecionado != null && LancamentoSelecionado.Status == StatusLancamento.Pago);

            // Carga inicial
            BuscarAsync();
        }

        private async Task BuscarAsync()
        {
            try
            {
                var dados = await _lancamentoService.ObterTodosAsync(DataInicio, DataFim);
                Lancamentos.Clear();
                foreach (var item in dados)
                {
                    Lancamentos.Add(item);
                }

                // --- CÁLCULOS DOS SALDOS ---
                // Saldo Realizado (Caixa Líquido: O que entrou de fato - O que saiu de fato)
                SaldoPeriodo = await _lancamentoService.ObterSaldoPeriodoAsync(DataInicio, DataFim);
                // Saldo Previsto (Competência: O que está agendado para vencer neste período)
                SaldoPrevisto = await _lancamentoService.ObterSaldoPrevistoAsync(DataInicio, DataFim);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar dados: {ex.Message}");
            }
        }

        private async void AbrirNovoLancamento()
        {
            try
            {
                var viewCadastro = _serviceProvider.GetRequiredService<LancamentoCadastroView>();

                if (viewCadastro.DataContext is LancamentoCadastroViewModel vm)
                {
                    await vm.CarregarListasAsync();
                }

                viewCadastro.Owner = System.Windows.Application.Current.MainWindow;
                viewCadastro.ShowDialog();

                await BuscarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditarLancamento()
        {
            if (LancamentoSelecionado == null) return;

            try
            {
                // Resolve a janela e a VM via DI
                var viewCadastro = _serviceProvider.GetRequiredService<LancamentoCadastroView>();
                var viewModel = (LancamentoCadastroViewModel)viewCadastro.DataContext;
                
                // CARREGA OS DADOS ANTES DE ABRIR
                await viewModel.CarregarListasAsync();
                await viewModel.CarregarParaEdicaoAsync(LancamentoSelecionado.Id);

                viewCadastro.Owner = System.Windows.Application.Current.MainWindow;
                viewCadastro.ShowDialog();

                // Recarrega a lista após fechar a janela de edição
                await BuscarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir edição: {ex.Message}");
            }
        }

        private async Task BaixarAsync()
        {
            if (LancamentoSelecionado == null) return;

            var msg = $"Deseja confirmar o pagamento/recebimento de:\n\n{LancamentoSelecionado.Descricao}\nValor: {LancamentoSelecionado.ValorOriginal:C2}?";
            if (MessageBox.Show(msg, "Confirmar Baixa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var dtoBaixa = new BaixarLancamentoDto
                    {
                        LancamentoId = LancamentoSelecionado.Id,
                        DataPagamento = DateTime.Now,
                        ValorPago = LancamentoSelecionado.ValorOriginal
                    };

                    await _lancamentoService.BaixarAsync(dtoBaixa);
                    await BuscarAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao baixar: {ex.Message}");
                }
            }
        }

        private async Task CancelarAsync()
        {
            if (LancamentoSelecionado == null) return;

            if (MessageBox.Show("Tem certeza que deseja CANCELAR este lançamento?", "Confirmar Cancelamento", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _lancamentoService.CancelarAsync(LancamentoSelecionado.Id);
                    await BuscarAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao cancelar: {ex.Message}");
                }
            }
        }

        private async Task EstornarLancamento()
        {
            if (LancamentoSelecionado == null) return;

            var msg = $"Deseja estornar (desfazer) o pagamento deste lançamento?\n\nEle voltará a ficar PENDENTE.\n\nDescrição: {LancamentoSelecionado.Descricao}";

            if (MessageBox.Show(msg, "Confirmar Estorno", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await _lancamentoService.EstornarLancamento(LancamentoSelecionado.Id);

                    MessageBox.Show("Pagamento estornado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                    await BuscarAsync(); // Atualiza a grid e os saldos
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao estornar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}