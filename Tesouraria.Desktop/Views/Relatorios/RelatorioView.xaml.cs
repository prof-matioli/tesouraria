using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Relatorios
{
    /// <summary>
    /// Lógica interna para RelatorioView.xaml
    /// </summary>
    public partial class RelatorioView : Window
    {
        public RelatorioView(RelatorioViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
