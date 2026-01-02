using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroCentroCustoFormWindow : Window
    {
        public CadastroCentroCustoFormWindow(CentroCustoCadastroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            if (viewModel != null) viewModel.RequestClose += () => this.Close();
        }

        public CentroCustoCadastroViewModel ViewModel => DataContext as CentroCustoCadastroViewModel;
    }
}