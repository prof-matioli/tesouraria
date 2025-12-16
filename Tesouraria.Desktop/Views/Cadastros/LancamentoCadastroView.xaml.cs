using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class LancamentoCadastroView : Window
    {
        public LancamentoCadastroView(LancamentoCadastroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Liga a Action da ViewModel ao método Close da janela
            viewModel.RequestClose = () => this.Close();
        }
    }
}