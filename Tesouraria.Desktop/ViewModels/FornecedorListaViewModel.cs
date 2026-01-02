using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq; // Necessário para o FirstOrDefault
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class FornecedorListaViewModel : ViewModelBase
    {
        private readonly IRepository<Fornecedor> _repository;
        private readonly IServiceProvider _serviceProvider;

        // A propriedade usada no ItemsSource do DataGrid é "Items"
        public ObservableCollection<Fornecedor> Items { get; } = new ObservableCollection<Fornecedor>();

        private Fornecedor? _selectedItem;
        public Fornecedor? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }

        public FornecedorListaViewModel(IRepository<Fornecedor> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));

            // CORREÇÃO 1: Recebe o ID do CommandParameter (int) e chama o formulário
            EditarCommand = new RelayCommand(param =>
            {
                if (param is int id) AbrirFormulario(id);
            });

            // CORREÇÃO 2: Recebe o ID do CommandParameter (int) e chama a exclusão
            ExcluirCommand = new RelayCommand(async param =>
            {
                if (param is int id) await Excluir(id);
            });

            BuscarCommand = new RelayCommand(async _ => await CarregarGrid());

            _ = CarregarGrid();
        }

        private void AbrirFormulario(int id)
        {
            try
            {
                var formWindow = _serviceProvider.GetRequiredService<CadastroFornecedorFormWindow>();
                _ = formWindow.ViewModel.Carregar(id);
                formWindow.ShowDialog();
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

        // CORREÇÃO 3: Método refatorado para aceitar o ID como parâmetro
        private async Task Excluir(int id)
        {
            // Busca o item na lista local apenas para pegar o Nome/Razão Social para a mensagem
            var itemParaExcluir = Items.FirstOrDefault(x => x.Id == id);
            var nome = itemParaExcluir?.RazaoSocial ?? "este fornecedor";

            var resultado = MessageBox.Show(
                $"Tem certeza que deseja excluir o fornecedor '{nome}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    // Usa o ID passado pelo botão para deletar
                    await _repository.DeleteAsync(id);

                    await CarregarGrid();
                    MessageBox.Show("Fornecedor excluído com sucesso.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir: O registro pode estar vinculado a lançamentos financeiros.\n\nDetalhes: {ex.Message}", "Erro");
                }
            }
        }
    }
}