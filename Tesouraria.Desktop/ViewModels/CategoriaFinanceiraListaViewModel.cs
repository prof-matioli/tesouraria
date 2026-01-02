using Microsoft.Extensions.DependencyInjection;
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
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }

        public CategoriaFinanceiraListaViewModel(IRepository<CategoriaFinanceira> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));

            // CORREÇÃO: Comandos agora aceitam o ID vindo do botão da linha
            EditarCommand = new RelayCommand(param =>
            {
                if (param is int id) AbrirFormulario(id);
            });

            ExcluirCommand = new RelayCommand(async param =>
            {
                if (param is int id) await Excluir(id);
            });

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
            try
            {
                Items.Clear();
                var dados = await _repository.GetAllAsync();
                foreach (var item in dados)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro ao carregar lista: " + ex.Message); }
        }

        // CORREÇÃO: Método alterado para receber ID
        private async Task Excluir(int id)
        {
            var itemParaExcluir = Items.FirstOrDefault(x => x.Id == id);
            var nome = itemParaExcluir?.Nome ?? "esta categoria";

            var confirm = MessageBox.Show($"Deseja excluir '{nome}'?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    await _repository.DeleteAsync(id);
                    await CarregarDados();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir: {ex.Message}\nVerifique se não há lançamentos vinculados.", "Erro");
                }
            }
        }
    }
}