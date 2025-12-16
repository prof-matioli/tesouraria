using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities; // Para acessar as listas de entidades simples
using Tesouraria.Domain.Interfaces;
using Tesouraria.Desktop.Core; 

namespace Tesouraria.Desktop.ViewModels
{
    public class LancamentoCadastroViewModel : ViewModelBase
    {
        private readonly ILancamentoService _lancamentoService;
        // Dependências para carregar os combos (supondo repositórios genéricos ou services específicos)
        private readonly IRepository<CentroCusto> _centroCustoRepo;
        private readonly IRepository<CategoriaFinanceira> _categoriaRepo;
        private readonly IRepository<Fiel> _fielRepo;
        private readonly IRepository<Fornecedor> _fornecedorRepo;

        // Propriedades de Binding
        private CriarLancamentoDto _dto;
        public CriarLancamentoDto Dto
        {
            get => _dto;
            set { _dto = value; OnPropertyChanged(); }
        }

        // Listas para ComboBoxes
        public ObservableCollection<CentroCusto> CentrosCusto { get; } = new();
        public ObservableCollection<CategoriaFinanceira> CategoriasFiltradas { get; } = new();
        public ObservableCollection<Fiel> Fieis { get; } = new();
        public ObservableCollection<Fornecedor> Fornecedores { get; } = new();

        // Controle de Tela
        private bool _isReceita;
        public bool IsReceita
        {
            get => _isReceita;
            set
            {
                if (SetProperty(ref _isReceita, value))
                {
                    Dto.Tipo = value ? TipoTransacao.Receita : TipoTransacao.Despesa;
                    IsDespesa = !value; // Atualiza o inverso
                    FiltrarCategorias();
                }
            }
        }

        private bool _isDespesa;
        public bool IsDespesa
        {
            get => _isDespesa;
            set => SetProperty(ref _isDespesa, value); // Apenas para controle de UI (Visibility)
        }

        // Cache de todas as categorias para evitar ir ao banco toda hora
        private List<CategoriaFinanceira> _todasCategorias = new();

        // Comandos
        public ICommand SalvarCommand { get; }
        public ICommand FecharCommand { get; } // Associaremos ao fechamento da janela

        // Action para fechar a janela via CodeBehind (padrão MVVM com Actions)
        public Action? RequestClose { get; set; }

        public LancamentoCadastroViewModel(
            ILancamentoService lancamentoService,
            IRepository<CentroCusto> centroCustoRepo,
            IRepository<CategoriaFinanceira> categoriaRepo,
            IRepository<Fiel> fielRepo,
            IRepository<Fornecedor> fornecedorRepo)
        {
            _lancamentoService = lancamentoService;
            _centroCustoRepo = centroCustoRepo;
            _categoriaRepo = categoriaRepo;
            _fielRepo = fielRepo;
            _fornecedorRepo = fornecedorRepo;

            // Inicializa DTO com valores padrão
            Dto = new CriarLancamentoDto
            {
                DataVencimento = DateTime.Today,
                Tipo = TipoTransacao.Despesa // Padrão inicial
            };

            IsDespesa = true;
            IsReceita = false;

            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            FecharCommand = new RelayCommand(_ => RequestClose?.Invoke());

            // Carrega dados iniciais
            CarregarListasAsync();
        }

        private async void CarregarListasAsync()
        {
            try
            {
                var custos = await _centroCustoRepo.GetAllAsync(); // Ajuste conforme seu repositório
                foreach (var c in custos) CentrosCusto.Add(c);

                _todasCategorias = (await _categoriaRepo.GetAllAsync()).ToList();
                FiltrarCategorias();

                var fieis = await _fielRepo.GetAllAsync();
                foreach (var f in fieis) Fieis.Add(f);

                var fornecedores = await _fornecedorRepo.GetAllAsync();
                foreach (var f in fornecedores) Fornecedores.Add(f);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar listas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarCategorias()
        {
            CategoriasFiltradas.Clear();
            var tipoAlvo = IsReceita ? TipoTransacao.Receita : TipoTransacao.Despesa;

            var filtradas = _todasCategorias.Where(c => c.Tipo == tipoAlvo).ToList();
            foreach (var c in filtradas) CategoriasFiltradas.Add(c);
        }

        private async Task SalvarAsync()
        {
            try
            {
                // Validação básica de UI
                if (Dto.Valor <= 0)
                {
                    MessageBox.Show("O valor deve ser maior que zero.", "Atenção");
                    return;
                }
                if (string.IsNullOrWhiteSpace(Dto.Descricao))
                {
                    MessageBox.Show("A descrição é obrigatória.", "Atenção");
                    return;
                }

                // Definir Usuário (Na fase de Auth você deve ter um Sessao.UsuarioId)
                // Dto.UsuarioId = SessaoSistema.UsuarioLogado.Id; 
                Dto.UsuarioId = 1; // HARDCODE TEMPORÁRIO PARA TESTE

                await _lancamentoService.RegistrarAsync(Dto);

                MessageBox.Show("Lançamento registrado com sucesso!", "Sucesso");
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}