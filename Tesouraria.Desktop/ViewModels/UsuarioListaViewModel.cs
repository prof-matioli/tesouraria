using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Tesouraria.Desktop.ViewModels
{
    public class UsuarioListaViewModel : ViewModelBase
    {
        private readonly IRepository<Usuario> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<Usuario> Items { get; } = new ObservableCollection<Usuario>();

        private Usuario? _selectedItem;
        public Usuario? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AlternarStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- FILTRO ---
        private bool _exibirInativos;
        public bool ExibirInativos
        {
            get => _exibirInativos;
            set
            {
                if (SetProperty(ref _exibirInativos, value))
                {
                    _ = CarregarGrid(); // Recarrega ao mudar o check
                }
            }
        }

        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand AlternarStatusCommand { get; } // Substitui o ExcluirCommand
        public ICommand BuscarCommand { get; }

        public UsuarioListaViewModel(IRepository<Usuario> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            // Por padrão não exibe os inativos (opcional)
            _exibirInativos = false;

            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));

            EditarCommand = new RelayCommand(_ => AbrirFormulario(SelectedItem!.Id), _ => SelectedItem != null);

            // Comando para Inativar ou Reativar
            AlternarStatusCommand = new RelayCommand(async param =>
            {
                if (param is int id) await AlternarStatus(id);
                else if (SelectedItem != null) await AlternarStatus(SelectedItem.Id);
            });

            BuscarCommand = new RelayCommand(async _ => await CarregarGrid());

            _ = CarregarGrid();
        }

        private void AbrirFormulario(int id)
        {
            try
            {
                var formView = _serviceProvider.GetRequiredService<UsuarioCadastroView>();

                // Configura o Owner para centralizar na janela principal
                formView.Owner = System.Windows.Application.Current.MainWindow;

                var vm = formView.DataContext as UsuarioCadastroViewModel;
                if (vm != null)
                {
                    _ = vm.Carregar(id);
                    formView.ShowDialog();
                    _ = CarregarGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir formulário: {ex.Message}");
            }
        }

        public async Task CarregarGrid()
        {
            try
            {
                Items.Clear();
                // Busca TODOS do banco
                var todosUsuarios = await _repository.GetAllAsync();

                if (todosUsuarios != null)
                {
                    // Lógica de Filtro:
                    // Se ExibirInativos == true, mostra tudo (todosUsuarios).
                    // Se ExibirInativos == false, filtra onde u.Ativo == true.
                    var filtrados = ExibirInativos
                        ? todosUsuarios
                        : todosUsuarios.Where(u => u.Ativo);

                    foreach (var item in filtrados.OrderBy(u => u.Nome))
                    {
                        Items.Add(item);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erro no grid: {ex.Message}"); }
        }

        private async Task AlternarStatus(int id)
        {
            try
            {
                var usuario = await _repository.GetByIdAsync(id);
                if (usuario == null) return;

                string acao = usuario.Ativo ? "INATIVAR" : "REATIVAR";
                string pergunta = $"Deseja realmente {acao} o usuário '{usuario.Nome}'?";

                if (MessageBox.Show(pergunta, "Alterar Status", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Inverte o status
                    usuario.Ativo = !usuario.Ativo;

                    // Salva a alteração
                    await _repository.UpdateAsync(usuario); // Use Update, não Delete

                    await CarregarGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao alterar status: {ex.Message}");
            }
        }
    }
}