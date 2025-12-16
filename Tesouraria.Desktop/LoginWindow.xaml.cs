using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
// Certifique-se que o namespace abaixo existe (onde está seu AuthService)
// using Tesouraria.Application.Services; 

namespace Tesouraria.Desktop
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;

        // O construtor recebe o serviço de autenticação via Injeção de Dependência
        public LoginWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            // Focar no campo de e-mail ao abrir
            TxtEmail.Focus();
        }

        private async void BtnEntrar_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();
            string senha = TxtSenha.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                TxtErro.Text = "Por favor, preencha e-mail e senha.";
                return;
            }

            try
            {
                BtnEntrar.IsEnabled = false;
                BtnEntrar.Content = "Verificando...";
                TxtErro.Text = string.Empty;

                // Chama o serviço (que agora retorna explicitamente UsuarioDTO)
                UsuarioDTO? usuarioLogado = await _authService.LoginAsync(email, senha);

                if (usuarioLogado != null)
                {
                    // Opcional: Você pode guardar o usuário logado em uma variável global/static se precisar
                    // Ex: App.UsuarioAtual = usuarioLogado;

                    AbrirSistemaPrincipal();
                }
                else
                {
                    TxtErro.Text = "E-mail ou senha inválidos.";
                }
            }
            catch (Exception ex)
            {
                TxtErro.Text = "Erro ao tentar realizar login. Tente novamente.";
                // Idealmente logar o erro: ex.Message
            }
            finally
            {
                BtnEntrar.IsEnabled = true;
                BtnEntrar.Content = "ENTRAR";
            }
        }

        private void AbrirSistemaPrincipal()
        {
            // Pega a instância do App
            if (System.Windows.Application.Current is Tesouraria.Desktop.App app)
            {
                // Solicita a MainWindow ao Container de DI
                var mainWindow = app.Host.Services.GetRequiredService<MainWindow>();

                mainWindow.Show();

                // Fecha a tela de login
                this.Close();
            }
        }

        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}