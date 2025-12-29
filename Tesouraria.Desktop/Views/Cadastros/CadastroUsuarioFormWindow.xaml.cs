using System.Windows;
using System.Windows.Controls;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroUsuarioFormWindow : Window
    {
        public CadastroUsuarioFormWindow(UsuarioCadastroViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Configura o fechamento automático
            if (viewModel != null)
            {
                viewModel.RequestClose += () => this.Close();
            }
        }

        private void TxtSenha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsuarioCadastroViewModel vm)
            {
                // Atualiza a propriedade SenhaEntrada no ViewModel manualmente
                vm.SenhaEntrada = ((PasswordBox)sender).Password;
            }
        }

        // Propriedade auxiliar que estava retornando null
        public UsuarioCadastroViewModel ViewModel => DataContext as UsuarioCadastroViewModel;

    }
}