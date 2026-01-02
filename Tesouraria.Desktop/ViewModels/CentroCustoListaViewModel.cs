using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class CentroCustoListaViewModel : ViewModelBase
    {
        private readonly IRepository<CentroCusto> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<CentroCusto> Items { get; } = new ObservableCollection<CentroCusto>();

        private CentroCusto? _selectedItem;
        public CentroCusto? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExcluirCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }

        public CentroCustoListaViewModel(IRepository<CentroCusto> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));
            EditarCommand = new RelayCommand(_ => AbrirFormulario(SelectedItem!.Id), _ => SelectedItem != null);
            ExcluirCommand = new RelayCommand(async _ => await Excluir(), _ => SelectedItem != null);
            BuscarCommand = new RelayCommand(async _ => await CarregarGrid());

            _ = CarregarGrid();
        }

        private void AbrirFormulario(int id)
        {
            try
            {
                var formWindow = _serviceProvider.GetRequiredService<CadastroCentroCustoFormWindow>();
                _ = formWindow.ViewModel.Carregar(id);
                formWindow.ShowDialog();
                _ = CarregarGrid();
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private async Task CarregarGrid()
        {
            Items.Clear();
            var todos = await _repository.GetAllAsync();

            // FILTRO MANUAL: Mostra apenas os ativos
            // Se você já arrumou o GetAllAsync no repositório, o .Where aqui é redundante mas seguro
            var ativos = todos.Where(x => x.Ativo).ToList();

            foreach (var item in ativos) Items.Add(item);
        }

        private async Task Excluir()
        {
            if (SelectedItem == null) return;
            if (MessageBox.Show($"Excluir {SelectedItem.Nome}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _repository.DeleteAsync(SelectedItem.Id);
                    await CarregarGrid();
                }
                catch (Exception ex) { MessageBox.Show("Erro ao excluir: " + ex.Message); }
            }
        }
    }
}