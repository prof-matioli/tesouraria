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

            // CORREÇÃO:
            // 1. Usar '+=' em vez de '=' para assinar o evento.
            // 2. Adicionar os parâmetros (s, e) pois é um EventHandler padrão.
            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}