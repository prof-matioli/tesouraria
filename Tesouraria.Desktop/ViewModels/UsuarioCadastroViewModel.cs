using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Utils;
using Tesouraria.Desktop.Core;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data.Repositories;

namespace Tesouraria.Desktop.ViewModels
{
    public class UsuarioCadastroViewModel : ViewModelBase
    {
        private readonly IRepository<Usuario> _repository;
        private readonly IRepository<Perfil> _perfilRepository;

        private Usuario _entidadeOriginal;

        private Usuario _entity;
        public Usuario Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value);
        }

        // Senha digitada pelo usuário (não salva no banco, vai para o Service gerar Hash)
        private string _senhaEntrada;
        public string SenhaEntrada
        {
            get => _senhaEntrada;
            set => SetProperty(ref _senhaEntrada, value);
        }

        // Título da janela (dinâmico)
        public string TituloJanela => Entity.Id == 0 ? "Novo Usuário" : "Editar Usuário";

        public ObservableCollection<Perfil> Perfis { get; } = new ObservableCollection<Perfil>();

        public ICommand SalvarCommand { get; }
        public ICommand FecharCommand { get; }

        // Evento para fechar a janela via CodeBehind
        public event Action RequestClose;
        /*
                public UsuarioCadastroViewModel(
                    IUsuarioService usuarioService,
                    IUsuarioRepository usuarioRepository,
                    IRepository<Perfil> perfilRepository)
                {
                    _usuarioService = usuarioService;
                    _usuarioRepository = usuarioRepository;
                    _perfilRepository = perfilRepository;

                    // Inicializa vazio
                    Entity = new Usuario { Ativo = true };

                    SalvarCommand = new RelayCommand(async _ => await Salvar());
                    FecharCommand = new RelayCommand(_ => RequestClose?.Invoke());
                }
        */

        public UsuarioCadastroViewModel(IRepository<Usuario> usuarioRepository,
            IRepository<Perfil> perfilRepository)
        {
            _repository = usuarioRepository;
            _perfilRepository = perfilRepository;

            // Inicializa vazio
            Entity = new Usuario { Ativo = true };

            SalvarCommand = new RelayCommand(async _ => await Salvar());
            FecharCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }


        // Método chamado pela Lista para preparar a tela
        public async Task Carregar(int id)
        {
            // 1. Carrega Combo de Perfis
            var listaPerfis = await _perfilRepository.GetAllAsync();
            Perfis.Clear();
            foreach (var p in listaPerfis) Perfis.Add(p);

            if (id > 0)
            {
                // 1. Busca a entidade original do banco (Rastreada pelo EF)
                _entidadeOriginal = await _repository.GetByIdAsync(id);

                if (_entidadeOriginal != null)
                {
                    // 2. CRUCIAL: Cria um CLONE para a tela editar.
                    // Isso quebra a referência com o EF enquanto editamos.
                    Entity = new Usuario
                    {
                        Id = _entidadeOriginal.Id,
                        Nome = _entidadeOriginal.Nome,
                        Email = _entidadeOriginal.Email,
                        Ativo = _entidadeOriginal.Ativo,
                        PerfilId = _entidadeOriginal.PerfilId,
                        SenhaHash = _entidadeOriginal.SenhaHash
                    };
                }
            }
            else
            {
                // Novo registro: A original é nula, a Entity é uma nova instância limpa
                _entidadeOriginal = null;
                Entity = new Usuario { Ativo = true }; // Modo Novo Cadastro
            }
        }

        private async Task Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Entity.Nome) || string.IsNullOrWhiteSpace(Entity.Email))
                {
                    MessageBox.Show("Nome e E-mail são obrigatórios.", "Atenção");
                    return;
                }

                if (Entity.Id == 0 && string.IsNullOrWhiteSpace(SenhaEntrada))
                {
                    MessageBox.Show("Para novos usuários, a senha é obrigatória.", "Atenção");
                    return;
                }

                if (Entity.PerfilId == 0)
                {
                    MessageBox.Show("Selecione um Perfil de acesso.", "Atenção");
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
                    Entity.SenhaHash = PasswordHelper.GenerateHash(SenhaEntrada);

                    await _repository.AddAsync(Entity);
                }
                else
                {
                    // --- ATUALIZAÇÃO (Edição) ---
                    _entidadeOriginal.Nome = Entity.Nome;
                    _entidadeOriginal.Email = Entity.Email;
                    _entidadeOriginal.Ativo = Entity.Ativo;
                    _entidadeOriginal.PerfilId = Entity.PerfilId;
                    _entidadeOriginal.PerfilId = Entity.PerfilId;
                    if (!string.IsNullOrWhiteSpace(SenhaEntrada))
                        _entidadeOriginal.SenhaHash = PasswordHelper.GenerateHash(SenhaEntrada);

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