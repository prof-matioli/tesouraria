using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroFielWindow : Window
    {
        public CadastroFielWindow(FielListaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public FielListaViewModel ViewModel => DataContext as FielListaViewModel;
    }
}