using System.Windows;
using System.Windows.Controls;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class UsuarioCadastroView : Window
    {
        public UsuarioCadastroView(UsuarioCadastroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Assina o evento para fechar a janela quando o ViewModel pedir
            viewModel.RequestClose += () => this.Close();
        }
        public UsuarioCadastroViewModel ViewModel => DataContext as UsuarioCadastroViewModel;

        private void TxtSenha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsuarioCadastroViewModel vm)
            {
                // Passa a senha do componente visual seguro para a ViewModel
                vm.SenhaEntrada = ((PasswordBox)sender).Password;
            }
        }
    }
}