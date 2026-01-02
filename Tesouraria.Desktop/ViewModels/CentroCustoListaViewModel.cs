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
    public class CentroCustoListaViewModel : ViewModelBase
    {
        private readonly IRepository<CentroCusto> _repository;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<CentroCusto> Items { get; } = new ObservableCollection<CentroCusto>();

        private CentroCusto? _selectedItem;
        public CentroCusto? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
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

            // CORREÇÃO: Comandos configurados para receber o ID do parâmetro (botão da linha)
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
                var formWindow = _serviceProvider.GetRequiredService<CadastroCentroCustoFormWindow>();
                _ = formWindow.ViewModel.Carregar(id);
                formWindow.ShowDialog();
                _ = CarregarGrid();
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private async Task CarregarGrid()
        {
            try
            {
                Items.Clear();
                var todos = await _repository.GetAllAsync();

                // Filtra apenas ativos, se necessário (ou traga todos se preferir)
                var ativos = todos.Where(x => x.Ativo).ToList();

                foreach (var item in ativos) Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar lista: " + ex.Message);
            }
        }

        // CORREÇÃO: Método alterado para receber o ID explicitamente
        private async Task Excluir(int id)
        {
            var itemParaExcluir = Items.FirstOrDefault(x => x.Id == id);
            var nome = itemParaExcluir?.Nome ?? "este centro de custo";

            if (MessageBox.Show($"Deseja realmente excluir '{nome}'?", "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _repository.DeleteAsync(id);
                    await CarregarGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir: {ex.Message}\nVerifique se não há lançamentos vinculados.", "Erro");
                }
            }
        }
    }
}