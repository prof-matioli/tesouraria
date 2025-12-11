using Serilog;
using System.Windows;

namespace Tesouraria.Desktop
{
    public partial class MainWindow : Window
    {
        // O construtor pode receber dependências agora!
        public MainWindow()
        {
            InitializeComponent();
            Log.Information("Aplicação Tesouraria iniciada com sucesso.");
        }
    }
}