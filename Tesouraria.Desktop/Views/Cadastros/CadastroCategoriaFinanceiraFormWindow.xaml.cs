using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroCategoriaFinanceiraFormWindow : Window
    {
        // Construtor recebe o ViewModel pronto
        public CadastroCategoriaFinanceiraFormWindow(CategoriaFinanceiraCadastroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Inscreve no evento para fechar a janela
            if (viewModel != null) viewModel.RequestClose += () => this.Close();
        }
        public CategoriaFinanceiraCadastroViewModel ViewModel => DataContext as CategoriaFinanceiraCadastroViewModel;

    }
}