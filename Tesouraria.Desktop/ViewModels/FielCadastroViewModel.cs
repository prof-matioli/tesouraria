using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core; // Onde está seu ViewModelBase e RelayCommand
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class FielCadastroViewModel : ViewModelBase
    {
        private readonly IRepository<Fiel> _repository;
        // Esta guarda a referência original do Banco de Dados (não ligada à tela)
        private Fiel _entidadeOriginal;

        private Fiel _entity;
        public Fiel Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value);
        }

        public ICommand SalvarCommand { get; }
        public ICommand FecharCommand { get; }

        public event Action RequestClose; // Evento para fechar a janela

        public FielCadastroViewModel(IRepository<Fiel> repository)
        {
            _repository = repository;
            Entity = new Fiel { Ativo = true};

            SalvarCommand = new RelayCommand(async _ => await Salvar());
            FecharCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        // Método chamado pela Lista para preparar a tela
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
                    Entity = new Fiel
                    {
                        Id = _entidadeOriginal.Id,
                        Nome = _entidadeOriginal.Nome,
                        CPF = _entidadeOriginal.CPF,
                        Telefone = _entidadeOriginal.Telefone,
                        Email = _entidadeOriginal.Email,
                        Endereco = _entidadeOriginal.Endereco,
                        DataNascimento = _entidadeOriginal.DataNascimento,
                        Dizimista = _entidadeOriginal.Dizimista,
                        Ativo = _entidadeOriginal.Ativo,
                        DataCriacao = _entidadeOriginal.DataCriacao
                    };
                }
            }
            else
            {
                // Novo registro: A original é nula, a Entity é uma nova instância limpa
                _entidadeOriginal = null;
                Entity = new Fiel { Ativo = true }; // Modo Novo Cadastro
            }
        }

        private async Task Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Entity.Nome))
                {
                    MessageBox.Show("Nome é obrigatório.", "Validação");
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
                    _entidadeOriginal.CPF = Entity.CPF;
                    _entidadeOriginal.Telefone = Entity.Telefone;
                    _entidadeOriginal.Email = Entity.Email;
                    _entidadeOriginal.Endereco = Entity.Endereco;
                    _entidadeOriginal.DataNascimento = Entity.DataNascimento;
                    _entidadeOriginal.Dizimista = Entity.Dizimista;

                    await _repository.UpdateAsync(_entidadeOriginal);
                }

                MessageBox.Show("Salvo com sucesso!", "Sucesso");
                RequestClose?.Invoke(); // Fecha a janela
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}");
            }
        }
    }
}