using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroCentroCustoWindow : Window
    {
        public CadastroCentroCustoWindow(CentroCustoListaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        public CentroCustoListaViewModel ViewModel => DataContext as CentroCustoListaViewModel;
    }
}