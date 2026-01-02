using System.ComponentModel;
using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views
{
    public partial class MainWindow : Window
    {
        // Construtor com Injeção de Dependência
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            // Vincula a ViewModel a esta View
            DataContext = viewModel;
        }

        // =============================================================
        // 1. Ação do Botão SAIR
        // =============================================================
        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            // Apenas manda fechar a janela.
            // Isso disparará automaticamente o evento 'Window_Closing' abaixo.
            // Não coloque MessageBox aqui para evitar a mensagem duplicada.
            this.Close();
        }
        
        // Lógica para confirmar saída ao clicar no "X" da janela
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show("Deseja realmente sair do sistema?",
                                         "Confirmação",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Cancela o fechamento
            }
        }
    }
}