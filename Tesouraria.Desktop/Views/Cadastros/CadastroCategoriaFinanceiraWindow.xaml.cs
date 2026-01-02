using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroCategoriaFinanceiraWindow : Window
    {
        public CadastroCategoriaFinanceiraWindow(CategoriaFinanceiraListaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        public CategoriaFinanceiraListaViewModel ViewModel => DataContext as CategoriaFinanceiraListaViewModel;
    }
}