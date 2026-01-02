using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class FielListaViewModel : ViewModelBase
    {
        private readonly IRepository<Fiel> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<Fiel> Items { get; } = new ObservableCollection<Fiel>();

        private Fiel? _selectedItem;
        public Fiel? SelectedItem
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

        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }

        public FielListaViewModel(IRepository<Fiel> repository, IServiceProvider serviceProvider)
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
                var formWindow = _serviceProvider.GetRequiredService<CadastroFielFormWindow>();

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
            if (SelectedItem == null) return;
            if (MessageBox.Show($"Tem certeza que deseja excluir {SelectedItem.Nome}?", "Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await _repository.DeleteAsync(SelectedItem.Id);
                await CarregarGrid();
            }
        }
    }
}