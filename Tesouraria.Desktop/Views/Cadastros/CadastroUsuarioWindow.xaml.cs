using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    /// <summary>
    /// Lógica interna para UsuarioListaView.xaml
    /// </summary>
    public partial class CadastroUsuarioWindow : Window
    {
        public CadastroUsuarioWindow(UsuarioListaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public UsuarioListaViewModel ViewModel => DataContext as UsuarioListaViewModel;
    }
}
