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
            }
        }

        // Totais
        private decimal _saldoPeriodo;
        public decimal SaldoPeriodo
        {
            get => _saldoPeriodo;
            set => SetProperty(ref _saldoPeriodo, value);
        }

        // Comandos
        public ICommand BuscarCommand { get; }
        public ICommand NovoCommand { get; }
        public ICommand BaixarCommand { get; }
        public ICommand CancelarCommand { get; }

        // CONSTRUTOR ATUALIZADO
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

                SaldoPeriodo = await _lancamentoService.ObterSaldoPeriodoAsync(DataInicio, DataFim);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar dados: {ex.Message}");
            }
        }

        // MÉTODO CORRIGIDO: Usa o Container de DI para criar a janela
        private void AbrirNovoLancamento()
        {
            try
            {
                // Pede ao sistema para criar a View (e ele cria a ViewModel e Repositórios automaticamente)
                var viewCadastro = _serviceProvider.GetRequiredService<LancamentoCadastroView>();

                // Exibe a janela como Modal
                viewCadastro.Owner = System.Windows.Application.Current.MainWindow; // Opcional: define o pai
                viewCadastro.ShowDialog();

                // Ao fechar, recarrega a grid
                BuscarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}