using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class LancamentoListaView : Window
    {
        public LancamentoListaView(LancamentoListaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}