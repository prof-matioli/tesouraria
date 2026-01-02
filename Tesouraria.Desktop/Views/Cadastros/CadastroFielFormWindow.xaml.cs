using System.Windows;
using Tesouraria.Desktop.ViewModels;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroFielFormWindow : Window
    {
        // Construtor com Injeção de Dependência
        public CadastroFielFormWindow(FielCadastroViewModel viewModel)
        {
            InitializeComponent();

            // ==========================================================
            // O ERRO ESTÁ AQUI: Você provavelmente esqueceu esta linha
            // ou ela está depois de alguma chamada que usa a ViewModel.
            // ==========================================================
            DataContext = viewModel;

            // Configura o fechamento automático
            if (viewModel != null)
            {
                viewModel.RequestClose += () => this.Close();
            }
        }

        // Propriedade auxiliar que estava retornando null
        public FielCadastroViewModel ViewModel => DataContext as FielCadastroViewModel;
    }
}