using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core; // Onde está ViewModelBase e RelayCommand
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class FornecedorCadastroViewModel : ViewModelBase
    {
        private readonly IRepository<Fornecedor> _repository;
        // Esta guarda a referência original do Banco de Dados (não ligada à tela)
        private Fornecedor _entidadeOriginal;

        // Entidade que está sendo editada
        private Fornecedor _entity;
        public Fornecedor Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value);
        }

        public ICommand SalvarCommand { get; }
        public ICommand FecharCommand { get; }

        public event Action RequestClose;

        public FornecedorCadastroViewModel(IRepository<Fornecedor> repository)
        {
            _repository = repository;
            Entity = new Fornecedor { Ativo = true };

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
                    Entity = new Fornecedor
                    {
                        Id = _entidadeOriginal.Id,
                        RazaoSocial = _entidadeOriginal.RazaoSocial,
                        NomeFantasia = _entidadeOriginal.NomeFantasia,
                        CNPJ = _entidadeOriginal.CNPJ,
                        Email=_entidadeOriginal.Email,
                        Telefone = _entidadeOriginal.Telefone,
                        Ativo = _entidadeOriginal.Ativo,
                        DataCriacao = _entidadeOriginal.DataCriacao
                    };
                }
            }
            else
            {
                // Novo registro: A original é nula, a Entity é uma nova instância limpa
                _entidadeOriginal = null;
                Entity = new Fornecedor { Ativo = true };
            }
        }

        private async Task Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Entity.RazaoSocial))
                {
                    MessageBox.Show("O Nome/Razão Social é obrigatório.", "Validação");
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

                    _entidadeOriginal.RazaoSocial = Entity.RazaoSocial;
                    _entidadeOriginal.NomeFantasia = Entity.NomeFantasia;
                    _entidadeOriginal.CNPJ = Entity.CNPJ;
                    _entidadeOriginal.Email= Entity.Email;
                    _entidadeOriginal.Telefone=Entity.Telefone;
                    _entidadeOriginal.DataAtualizacao = DateTime.Now;

                    await _repository.UpdateAsync(_entidadeOriginal);
                }

                MessageBox.Show("Fornecedor salvo com sucesso!", "Sucesso");
                RequestClose?.Invoke(); // Pede para a janela fechar
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}");
            }
        }
    }
}