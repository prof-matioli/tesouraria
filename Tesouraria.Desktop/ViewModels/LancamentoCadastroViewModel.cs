using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class LancamentoCadastroViewModel : ViewModelBase
    {
        private readonly ILancamentoService _lancamentoService;
        private readonly IRepository<CategoriaFinanceira> _categoriaRepo;
        private readonly IRepository<CentroCusto> _centroCustoRepo;
        private readonly IRepository<Fiel> _fielRepo;
        private readonly IRepository<Fornecedor> _fornecedorRepo;

        public event EventHandler? RequestClose;

        private CriarLancamentoDto _entity;
        public CriarLancamentoDto Entity
        {
            get => _entity;
            set
            {
                SetProperty(ref _entity, value);
                OnPropertyChanged(nameof(TipoSelecionado));
                OnPropertyChanged(nameof(IsReceita));
                OnPropertyChanged(nameof(IsDespesa));
            }
        }

        private List<CategoriaFinanceira> _todasCategorias = new();
        public ObservableCollection<CategoriaFinanceira> CategoriasFiltradas { get; } = new();
        public ObservableCollection<CentroCusto> CentrosCusto { get; } = new();
        public ObservableCollection<Fiel> Fieis { get; } = new();
        public ObservableCollection<Fornecedor> Fornecedores { get; } = new();

        private int _idEdicao = 0;
        public string TituloJanela => _idEdicao == 0 ? "Novo Lançamento" : "Editar Lançamento";

        public TipoTransacao TipoSelecionado
        {
            get => Entity.Tipo;
            set
            {
                if (Entity.Tipo != value)
                {
                    Entity.Tipo = value;
                    OnPropertyChanged(nameof(TipoSelecionado));
                    OnPropertyChanged(nameof(IsReceita));
                    OnPropertyChanged(nameof(IsDespesa));

                    FiltrarCategorias();
                    Entity.CategoriaId = 0;
                    OnPropertyChanged(nameof(Entity));
                }
            }
        }

        public bool IsReceita
        {
            get => TipoSelecionado == TipoTransacao.Receita;
            set { if (value) TipoSelecionado = TipoTransacao.Receita; }
        }

        public bool IsDespesa
        {
            get => TipoSelecionado == TipoTransacao.Despesa;
            set { if (value) TipoSelecionado = TipoTransacao.Despesa; }
        }

        public ICommand SalvarCommand { get; }
        public ICommand FecharCommand { get; }

        public LancamentoCadastroViewModel(
            ILancamentoService lancamentoService,
            IRepository<CategoriaFinanceira> categoriaRepo,
            IRepository<CentroCusto> centroCustoRepo,
            IRepository<Fiel> fielRepo,
            IRepository<Fornecedor> fornecedorRepo)
        {
            _lancamentoService = lancamentoService;
            _categoriaRepo = categoriaRepo;
            _centroCustoRepo = centroCustoRepo;
            _fielRepo = fielRepo;
            _fornecedorRepo = fornecedorRepo;

            // ALTERAÇÃO AQUI: Padrão agora é Receita
            Entity = new CriarLancamentoDto { DataVencimento = DateTime.Now, Tipo = TipoTransacao.Receita };

            SalvarCommand = new RelayCommand(async _ => await Salvar());
            FecharCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
        }

        public async Task Carregar(int id)
        {
            try
            {
                _idEdicao = id;
                OnPropertyChanged(nameof(TituloJanela));

                await CarregarListasAuxiliares();

                if (id == 0)
                {
                    // ALTERAÇÃO AQUI: Novo lançamento inicia como Receita
                    Entity = new CriarLancamentoDto
                    {
                        DataVencimento = DateTime.Now,
                        Tipo = TipoTransacao.Receita,
                        UsuarioId = SessaoSistema.UsuarioLogado?.Id ?? 1
                    };
                }
                else
                {
                    var dto = await _lancamentoService.ObterPorIdAsync(id);
                    if (dto != null)
                    {
                        Entity = new CriarLancamentoDto
                        {
                            Descricao = dto.Descricao,
                            Valor = dto.ValorOriginal,
                            DataVencimento = dto.DataVencimento,
                            Tipo = dto.Tipo,
                            Observacao = dto.Observacao,
                            CategoriaId = dto.CategoriaId,
                            CentroCustoId = dto.CentroCustoId,
                            FielId = dto.FielId,
                            FornecedorId = dto.FornecedorId,
                            UsuarioId = SessaoSistema.UsuarioLogado?.Id ?? 1
                        };
                    }
                }

                FiltrarCategorias();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar: {ex.Message}");
            }
        }

        private async Task CarregarListasAuxiliares()
        {
            var cats = await _categoriaRepo.GetAllAsync();
            _todasCategorias = cats.ToList();

            CentrosCusto.Clear();
            var custos = await _centroCustoRepo.GetAllAsync();
            foreach (var c in custos) CentrosCusto.Add(c);

            Fieis.Clear();
            var fieis = await _fielRepo.GetAllAsync();
            foreach (var f in fieis) Fieis.Add(f);

            Fornecedores.Clear();
            var forns = await _fornecedorRepo.GetAllAsync();
            foreach (var f in forns) Fornecedores.Add(f);
        }

        private void FiltrarCategorias()
        {
            CategoriasFiltradas.Clear();
            var filtradas = _todasCategorias.Where(c => c.Tipo == TipoSelecionado);
            foreach (var cat in filtradas) CategoriasFiltradas.Add(cat);
        }

        private async Task Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Entity.Descricao)) { MessageBox.Show("Informe a descrição."); return; }
                if (Entity.Valor <= 0) { MessageBox.Show("O valor deve ser maior que zero."); return; }
                if (Entity.CategoriaId <= 0) { MessageBox.Show("Selecione uma Categoria."); return; }
                if (Entity.CentroCustoId <= 0) { MessageBox.Show("Selecione um Centro de Custo."); return; }

                if (IsReceita) Entity.FornecedorId = null;
                if (IsDespesa) Entity.FielId = null;

                Entity.UsuarioId = SessaoSistema.UsuarioLogado?.Id ?? 1;

                if (_idEdicao == 0)
                {
                    await _lancamentoService.RegistrarAsync(Entity);
                    MessageBox.Show("Registrado com sucesso!");
                }
                else
                {
                    await _lancamentoService.AtualizarAsync(_idEdicao, Entity);
                    MessageBox.Show("Atualizado com sucesso!");
                }

                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}");
            }
        }
    }
}