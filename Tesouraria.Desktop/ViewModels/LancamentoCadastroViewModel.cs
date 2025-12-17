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

        private int _idEdicao = 0;
        public bool ModoEdicao => _idEdicao > 0; // Útil se quiser mudar o título da janela via Binding

        // Propriedades de Binding
        private CriarLancamentoDto _dto;

        // Listas para ComboBoxes
        public ObservableCollection<CentroCusto> CentrosCusto { get; } = new();
        public ObservableCollection<CategoriaFinanceira> CategoriasFiltradas { get; } = new();
        public ObservableCollection<Fiel> Fieis { get; } = new();
        public ObservableCollection<Fornecedor> Fornecedores { get; } = new();

        // Controle de Tela
        private bool _isReceita;


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
        }

        public CriarLancamentoDto Dto
        {
            get => _dto;
            set { _dto = value; OnPropertyChanged(); }
        }

        public bool IsReceita
        {
            get => _isReceita;
            set
            {
                if (SetProperty(ref _isReceita, value))
                {
                    // 1. Atualiza o tipo no DTO
                    Dto.Tipo = value ? TipoTransacao.Receita : TipoTransacao.Despesa;

                    // 2. Atualiza o booleano inverso (para controle visual)
                    IsDespesa = !value;

                    // 3. Atualiza a lista de categorias disponíveis
                    FiltrarCategorias();

                    // 4. IMPORTANTE: Limpa a seleção anterior para evitar erro de categoria incompatível
                    // Só limpamos se estivermos mudando na tela (não durante o carregamento inicial)
                    if (CategoriasFiltradas.All(c => c.Id != Dto.CategoriaId))
                    {
                        Dto.CategoriaId = 0;
                        OnPropertyChanged(nameof(Dto)); // Avisa a tela para limpar o ComboBox
                    }
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

        // MÉTODO NOVO: Chamado pela lista para preencher a tela
        public async Task CarregarParaEdicaoAsync(int id)
        {
            var lancamento = await _lancamentoService.ObterPorIdAsync(id);
            if (lancamento == null) return;

            _idEdicao = lancamento.Id;

            // Configura os booleans para a UI reagir (Visibilidade dos campos)
            if (lancamento.Tipo == TipoTransacao.Receita)
            {
                IsReceita = true; // Isso dispara a lógica de filtrar categorias
            }
            else
            {
                IsDespesa = true;
            }

            // Preenche o DTO que está ligado aos campos da tela
            Dto = new CriarLancamentoDto
            {
                Descricao = lancamento.Descricao,
                Valor = lancamento.ValorOriginal,
                DataVencimento = lancamento.DataVencimento,
                Tipo = lancamento.Tipo,
                CategoriaId = lancamento.CategoriaId,
                CentroCustoId = lancamento.CentroCustoId,
                FielId = lancamento.FielId,
                FornecedorId = lancamento.FornecedorId,
                Observacao = lancamento.Observacao,
                UsuarioId = SessaoSistema.UsuarioId
            };

            // Força a notificação para a UI atualizar os Combos
            OnPropertyChanged(nameof(Dto));
        }


        public async Task CarregarListasAsync()
        {
            try
            {
                // Limpa as listas antes de carregar para evitar duplicação se chamado 2x
                CentrosCusto.Clear();
                CategoriasFiltradas.Clear();
                _todasCategorias.Clear();
                Fieis.Clear();
                Fornecedores.Clear();

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
                // 1. Validações Básicas de Interface
                if (Dto.Valor <= 0)
                {
                    MessageBox.Show("O valor do lançamento deve ser maior que zero.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Dto.Descricao))
                {
                    MessageBox.Show("Por favor, informe uma descrição para o lançamento.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validação importante para evitar erro de FK (caso os combos não tenham carregado ou usuário não selecionou)
                if (Dto.CategoriaId <= 0)
                {
                    MessageBox.Show("Selecione uma Categoria Financeira.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Dto.CentroCustoId <= 0)
                {
                    MessageBox.Show("Selecione um Centro de Custo.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Validação de Segurança (Sessão)
                // Isso resolve o erro "FK_Lancamentos_Usuarios" definitivamente
                if (SessaoSistema.UsuarioId <= 0)
                {
                    MessageBox.Show("Sessão expirada ou usuário não identificado.\nPor favor, feche o sistema e faça login novamente.",
                                    "Erro de Segurança", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Atribui o ID do usuário logado ao DTO
                Dto.UsuarioId = SessaoSistema.UsuarioId;

                // 3. Decisão: Inserir ou Atualizar
                if (_idEdicao == 0)
                {
                    // --- MODO CRIAÇÃO ---
                    await _lancamentoService.RegistrarAsync(Dto);
                    MessageBox.Show("Lançamento registrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // --- MODO EDIÇÃO ---
                    await _lancamentoService.AtualizarAsync(_idEdicao, Dto);
                    MessageBox.Show("Lançamento atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // 4. Fecha a janela
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                // 5. Tratamento de Erro Robusto (Mostra a causa raiz do SQL Server)
                var mensagemErro = ex.Message;

                // Verifica se existe uma exceção interna (o erro real do banco)
                if (ex.InnerException != null)
                {
                    mensagemErro += $"\n\nDetalhe Técnico: {ex.InnerException.Message}";

                    // Às vezes o erro real está ainda mais fundo
                    if (ex.InnerException.InnerException != null)
                    {
                        mensagemErro += $"\n\nRaiz do Problema: {ex.InnerException.InnerException.Message}";
                    }
                }

                MessageBox.Show($"Ocorreu um erro ao salvar:\n{mensagemErro}", "Erro de Banco de Dados", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}