using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroFornecedorFormWindow : Window
    {
        public CadastroFornecedorFormWindow(FornecedorCadastroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }

        // --- NECESSÁRIO PARA A LISTA ACESSAR O MÉTODO .Carregar() ---
        public FornecedorCadastroViewModel ViewModel => DataContext as FornecedorCadastroViewModel;
    }
}