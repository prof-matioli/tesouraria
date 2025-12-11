using System.Windows;
using Tesouraria.Application.Interfaces;

namespace Tesouraria.Desktop
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly MainWindow _mainWindow;

        // Injetamos o AuthService e a MainWindow (que será aberta se logar com sucesso)
        public LoginWindow(IAuthService authService, MainWindow mainWindow)
        {
            InitializeComponent();
            _authService = authService;
            _mainWindow = mainWindow;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtErro.Text = "Verificando...";
            btnLogin.IsEnabled = false;

            try
            {
                var usuario = await _authService.LoginAsync(txtEmail.Text, txtSenha.Password);

                if (usuario != null)
                {
                    // Login Sucesso
                    _mainWindow.Show();
                    this.Close();
                }
                else
                {
                    txtErro.Text = "E-mail ou senha inválidos.";
                }
            }
            catch (Exception ex)
            {
                txtErro.Text = "Erro ao tentar logar.";
                // Logar o erro real aqui com Serilog futuramente
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }
    }
}