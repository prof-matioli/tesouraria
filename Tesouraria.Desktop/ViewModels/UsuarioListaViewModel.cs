using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros; // Necessário para abrir a janela
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection; // Para resolver a janela de cadastro

namespace Tesouraria.Desktop.ViewModels
{
    public class UsuarioListaViewModel : ViewModelBase
    {
        private readonly IRepository<Usuario> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<Usuario> Items { get; } = new ObservableCollection<Usuario>();

        // Propriedade para controlar o item selecionado no Grid
        private Usuario? _selectedItem;
        public Usuario? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                // Atualiza o status dos botões
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExcluirCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand CarregarCommand { get; }
        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }

        public UsuarioListaViewModel(IRepository<Usuario> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));

            // Só habilita Editar se tiver item selecionado
            EditarCommand = new RelayCommand(_ => AbrirFormulario(SelectedItem!.Id), _ => SelectedItem != null);

            ExcluirCommand = new RelayCommand(async _ => await Excluir(), _ => SelectedItem != null);
            BuscarCommand = new RelayCommand(async _ => await CarregarGrid());

            _ = CarregarGrid();
        }

        private void AbrirFormulario(int id)
        {
            try
            {
                // 1. Cria a Janela de FORMULÁRIO (FormWindow)
                var formWindow = _serviceProvider.GetRequiredService<CadastroUsuarioFormWindow>();

                // 2. Carrega os dados (0 = Novo, >0 = Edição)
                // O ConfigureAwait(false) evita travar a UI enquanto busca no banco
                // Mas aqui usamos Task.Run para garantir que o load ocorra
                _ = formWindow.ViewModel.Carregar(id);

                // 3. Abre travando a tela (Dialog)
                formWindow.ShowDialog();

                // 4. Quando fechar, atualiza o grid
                _ = CarregarGrid();
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
                var dados = await _repository.GetAllAsync();
                if (dados != null)
                {
                    foreach (var item in dados) Items.Add(item);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Erro no grid: {ex.Message}"); }
        }



        private async Task Excluir()
        {
            if (_selectedItem == null) return;

            if (MessageBox.Show($"Deseja excluir o usuário {_selectedItem.Nome}?", "Exclusão",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await _repository.DeleteAsync(_selectedItem.Id);
                await CarregarGrid();
            }
        }
    }
}