using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq; // Necessário para o FirstOrDefault
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.Services;
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

        // SelectedItem mantido apenas se você quiser usar para outras coisas, 
        // mas não é mais obrigatório para os botões da linha.
        private Fiel? _selectedItem;
        public Fiel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand BuscarCommand { get; }

        public FielListaViewModel(IRepository<Fiel> repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;

            // Novo: Passa 0
            NovoCommand = new RelayCommand(_ => AbrirFormulario(0));

            // CORREÇÃO AQUI: 
            // O comando agora aceita o 'id' (object) passado pelo CommandParameter do XAML
            // Removemos a validação "SelectedItem != null" pois o botão está na linha, o item existe.
            EditarCommand = new RelayCommand(param =>
            {
                if (param is int id) AbrirFormulario(id);
            });

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
                var formWindow = _serviceProvider.GetRequiredService<CadastroFielFormWindow>();

                // Carrega os dados na ViewModel do formulário
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

        // Método Excluir alterado para receber o ID
        private async Task Excluir(int id)
        {
            // Busca o item na lista local apenas para mostrar o nome na mensagem
            var itemParaExcluir = Items.FirstOrDefault(x => x.Id == id);
            var nome = itemParaExcluir?.Nome ?? "este item";

            if (MessageBox.Show($"Tem certeza que deseja excluir {nome}?", "Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _repository.DeleteAsync(id); // Usa o ID passado pelo botão
                    await CarregarGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir: {ex.Message}");
                }
            }
        }
    }
}