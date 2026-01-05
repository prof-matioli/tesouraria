using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Services;
using Tesouraria.Desktop.Core;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class ImportacaoExtratoViewModel : ViewModelBase
    {
        private readonly ExtratoService _extratoService;
        private readonly ILancamentoService _lancamentoService;
        private readonly IRepository<CategoriaFinanceira> _categoriaRepository;
        private readonly IRepository<CentroCusto> _centroCustoRepository;

        // --- Listas para a Tela ---
        public ObservableCollection<TransacaoExtratoDto> Transacoes { get; } = new();

        // Listas separadas para facilitar a seleção correta
        public ObservableCollection<CategoriaFinanceira> CategoriasReceita { get; } = new();
        public ObservableCollection<CategoriaFinanceira> CategoriasDespesa { get; } = new();

        public ObservableCollection<CentroCusto> CentrosCusto { get; } = new();

        // --- Seleções do Usuário ---

        // 1. Categoria padrão para Créditos (Entradas)
        private CategoriaFinanceira? _categoriaReceitaPadrao;
        public CategoriaFinanceira? CategoriaReceitaPadrao
        {
            get => _categoriaReceitaPadrao;
            set
            {
                SetProperty(ref _categoriaReceitaPadrao, value);
                (ProcessarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // 2. Categoria padrão para Débitos (Saídas)
        private CategoriaFinanceira? _categoriaDespesaPadrao;
        public CategoriaFinanceira? CategoriaDespesaPadrao
        {
            get => _categoriaDespesaPadrao;
            set
            {
                SetProperty(ref _categoriaDespesaPadrao, value);
                (ProcessarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private CentroCusto? _centroCustoPadrao;
        public CentroCusto? CentroCustoPadrao
        {
            get => _centroCustoPadrao;
            set
            {
                SetProperty(ref _centroCustoPadrao, value);
                (ProcessarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Comandos ---
        public ICommand SelecionarArquivoCommand { get; }
        public ICommand ProcessarCommand { get; }

        public ImportacaoExtratoViewModel(
            ExtratoService extratoService,
            ILancamentoService lancamentoService,
            IRepository<CategoriaFinanceira> catRepo,
            IRepository<CentroCusto> custoRepo)
        {
            _extratoService = extratoService;
            _lancamentoService = lancamentoService;
            _categoriaRepository = catRepo;
            _centroCustoRepository = custoRepo;

            SelecionarArquivoCommand = new RelayCommand(_ => SelecionarArquivo());

            // Validação mais rigorosa: Precisa ter ambas as categorias selecionadas
            ProcessarCommand = new RelayCommand(
                async _ => await SalvarNoBanco(),
                _ => Transacoes.Any() && CategoriaReceitaPadrao != null && CategoriaDespesaPadrao != null && CentroCustoPadrao != null
            );

            _ = CarregarDadosIniciais();
        }

        private async Task CarregarDadosIniciais()
        {
            try
            {
                var cats = await _categoriaRepository.GetAllAsync();
                var custos = await _centroCustoRepository.GetAllAsync();

                CategoriasReceita.Clear();
                CategoriasDespesa.Clear();

                // Separa as categorias por tipo nas listas
                foreach (var c in cats.OrderBy(x => x.Nome))
                {
                    if (c.Tipo == TipoTransacao.Receita)
                        CategoriasReceita.Add(c);
                    else
                        CategoriasDespesa.Add(c);
                }

                CentrosCusto.Clear();
                foreach (var c in custos.OrderBy(x => x.Nome)) CentrosCusto.Add(c);
            }
            catch { }
        }

        private void SelecionarArquivo()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Arquivos PDF (*.pdf)|*.pdf",
                Title = "Selecione o Extrato Sicoob"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var itens = _extratoService.LerArquivoPdf(dialog.FileName);

                    Transacoes.Clear();
                    foreach (var item in itens) Transacoes.Add(item);

                    if (!itens.Any())
                        MessageBox.Show("Nenhuma transação válida encontrada no PDF.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                    (ProcessarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler PDF: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task SalvarNoBanco()
        {
            if (MessageBox.Show($"Confirma a importação de {Transacoes.Count} lançamentos?\nEles serão cadastrados como 'Pagos'.",
                "Confirmar Importação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                int count = 0;
                foreach (var t in Transacoes)
                {
                    // Lógica para decidir qual categoria usar
                    bool isReceita = t.Tipo == 'C';
                    int categoriaId = isReceita ? CategoriaReceitaPadrao!.Id : CategoriaDespesaPadrao!.Id;

                    var dto = new CriarLancamentoDto
                    {
                        Descricao = t.Historico,
                        Valor = t.Valor,
                        DataVencimento = t.Data,
                        Tipo = isReceita ? TipoTransacao.Receita : TipoTransacao.Despesa,

                        FormaPagamento = FormaPagamento.Pix,

                        // Usa a categoria correta baseada no tipo da transação
                        CategoriaId = categoriaId,

                        CentroCustoId = CentroCustoPadrao!.Id,
                        UsuarioId = 1,
                        Observacao = "Importado Automaticamente via PDF Sicoob"
                    };

                    await _lancamentoService.RegistrarAsync(dto);
                    count++;
                }

                MessageBox.Show($"{count} lançamentos importados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                Transacoes.Clear();
                (ProcessarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar lançamentos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}