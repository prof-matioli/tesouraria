using System.ComponentModel;
using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views
{
    /// <summary>
    /// Lógica interna para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            // ESTA É A LINHA MÁGICA:
            // Conecta o XAML (View) ao Código C# (ViewModel)
            DataContext = viewModel;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Exibe a mensagem de confirmação
            var result = MessageBox.Show("Deseja realmente sair do sistema?",
                                         "Confirmação",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            // Se o usuário clicar em 'Não', cancela o fechamento
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
