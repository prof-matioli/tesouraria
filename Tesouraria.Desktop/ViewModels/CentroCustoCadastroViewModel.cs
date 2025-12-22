using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class CentroCustoCadastroViewModel : ViewModelBase
    {
        private readonly IRepository<CentroCusto> _repository;

        // Esta guarda a referência original do Banco de Dados (não ligada à tela)
        private CentroCusto _entidadeOriginal;

        private CentroCusto _entity;
        public CentroCusto Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value);
        }

        public ICommand SalvarCommand { get; }
        public ICommand FecharCommand { get; }
        public event Action RequestClose;

        public CentroCustoCadastroViewModel(IRepository<CentroCusto> repository)
        {
            _repository = repository;

            // Inicializa a cópia de trabalho vazia
            Entity = new CentroCusto { Ativo = true };

            SalvarCommand = new RelayCommand(async _ => await Salvar());
            FecharCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        public async Task Carregar(int id)
        {
            if (id > 0)
            {
                // 1. Busca a entidade original do banco (Rastreada pelo EF)
                _entidadeOriginal = await _repository.GetByIdAsync(id);

                if (_entidadeOriginal != null)
                {
                    // 2. CRUCIAL: Cria um CLONE para a tela editar.
                    // Isso quebra a referência com o EF enquanto editamos.
                    Entity = new CentroCusto
                    {
                        Id = _entidadeOriginal.Id,
                        Nome = _entidadeOriginal.Nome,
                        Descricao = _entidadeOriginal.Descricao,
                        Ativo = _entidadeOriginal.Ativo,
                        DataCriacao = _entidadeOriginal.DataCriacao
                    };
                }
            }
            else
            {
                // Novo registro: A original é nula, a Entity é uma nova instância limpa
                _entidadeOriginal = null;
                Entity = new CentroCusto { Ativo = true };
            }
        }

        private async Task Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Entity.Nome))
                {
                    MessageBox.Show("O Nome é obrigatório.", "Validação");
                    return;
                }

                // Atualiza datas da BaseEntity
                Entity.DataAtualizacao = DateTime.Now;

                if (Entity.Id == 0)
                {
                    // --- INSERÇÃO (Novo) ---
                    // Como é novo, a Entity da tela já é o objeto que queremos salvar
                    Entity.DataCriacao = DateTime.Now;
                    Entity.DataAtualizacao = DateTime.Now;

                    await _repository.AddAsync(Entity);
                }
                else
                {
                    // --- ATUALIZAÇÃO (Edição) ---
                    // Agora pegamos os dados da CÓPIA (Entity) e jogamos na ORIGINAL (_entidadeOriginal)
                    // Somente agora o EF fica sabendo das mudanças.

                    _entidadeOriginal.Nome = Entity.Nome;
                    _entidadeOriginal.Descricao = Entity.Descricao;
                    _entidadeOriginal.DataAtualizacao = DateTime.Now;

                    await _repository.UpdateAsync(_entidadeOriginal);
                }

                MessageBox.Show("Salvo com sucesso!");
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}");
            }
        }
    }
}