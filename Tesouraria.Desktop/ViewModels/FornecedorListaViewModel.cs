using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views.Cadastros; // Para achar o FormWindow
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Desktop.ViewModels
{
    public class FornecedorListaViewModel : ViewModelBase
    {
        private readonly IRepository<Fornecedor> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<Fornecedor> Items { get; } = new ObservableCollection<Fornecedor>();

        private Fornecedor? _selectedItem;
        public Fornecedor? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                // Atualiza botões
                (EditarCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExcluirCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
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

            EditarCommand = new RelayCommand(
                _ => AbrirFormulario(SelectedItem!.Id),
                _ => SelectedItem != null
            );

            ExcluirCommand = new RelayCommand(async _ => await Excluir(), _ => SelectedItem != null);
            BuscarCommand = new RelayCommand(async _ => await CarregarGrid());

            _ = CarregarGrid();
        }

        private void AbrirFormulario(int id)
        {
            try
            {
                // Pede a Janela de FORMULÁRIO (FormWindow)
                var formWindow = _serviceProvider.GetRequiredService<CadastroFornecedorFormWindow>();

                // Carrega os dados na ViewModel do formulário
                _ = formWindow.ViewModel.Carregar(id);

                // Abre travando a tela
                formWindow.ShowDialog();

                // Atualiza o grid ao voltar
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
            // Validação de segurança
            if (SelectedItem == null) return;

            // Pergunta de confirmação
            var resultado = MessageBox.Show(
                $"Tem certeza que deseja excluir o fornecedor '{SelectedItem.RazaoSocial}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    // Chama o repositório para deletar pelo ID
                    await _repository.DeleteAsync(SelectedItem.Id);

                    // Recarrega o grid para sumir com o item excluído
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