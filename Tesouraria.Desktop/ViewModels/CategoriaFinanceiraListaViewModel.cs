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
    public class CategoriaFinanceiraListaViewModel : ViewModelBase
    {
        private readonly IRepository<CategoriaFinanceira> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<CategoriaFinanceira> Items { get; } = new ObservableCollection<CategoriaFinanceira>();

        private CategoriaFinanceira? _selectedItem;
        public CategoriaFinanceira? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExcluirCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand CarregarCommand { get; }
        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }


        public CategoriaFinanceiraListaViewModel(IRepository<CategoriaFinanceira> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;
            CarregarCommand = new RelayCommand(async _ => await CarregarDados());
            ExcluirCommand = new RelayCommand(async _ => await Excluir(), _ => SelectedItem != null);
            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));
            EditarCommand = new RelayCommand(_ => AbrirFormulario(SelectedItem!.Id), _ => SelectedItem != null);
            BuscarCommand = new RelayCommand(async _ => await CarregarDados());

            _ = CarregarDados();

        }

        private void AbrirFormulario(int id)
        {
            try
            {
                var formWindow = _serviceProvider.GetRequiredService<CadastroCategoriaFinanceiraFormWindow>();
                _ = formWindow.ViewModel.Carregar(id);
                formWindow.ShowDialog();
                _ = CarregarDados();
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        public async Task CarregarDados()
        {
            Items.Clear();
            var dados = await _repository.GetAllAsync();
            foreach (var item in dados)
            {
                Items.Add(item);
            }
        }

        private async Task Excluir()
        {
            if (SelectedItem == null) return;

            var confirm = MessageBox.Show($"Deseja excluir '{SelectedItem.Nome}'?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                await _repository.DeleteAsync(SelectedItem.Id);
                await CarregarDados();
            }
        }
    }
}