using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroFornecedorWindow : Window
    {
        public CadastroFornecedorWindow(FornecedorListaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        // Permite acessar a ViewModel se necessário
        public FornecedorListaViewModel ViewModel => DataContext as FornecedorListaViewModel;
    }
}