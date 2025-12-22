using System.Windows;
using Tesouraria.Desktop.ViewModels;
using System.Windows.Input;

namespace Tesouraria.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();

            // Conecta a ViewModel
            DataContext = viewModel;

            // Inscreve-se no evento para fechar a janela quando o login for bem-sucedido
            // Isso mantém a regra de que a ViewModel não conhece a Window diretamente
            viewModel.RequestClose += () => this.Close();
        }
        // Permite arrastar a janela (necessário pois WindowStyle="None")
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

    }
}