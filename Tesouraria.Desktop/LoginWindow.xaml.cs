using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Domain.Entities;
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

            // Valores padrão para facilitar testes (remova em produção)
            TxtEmail.Text = "admin@paroquia.com";
            TxtSenha.Password = "123456";

            // Focar no campo de e-mail ao abrir
            TxtEmail.Focus();
        }

        // PERMITE ARRASTAR A JANELA
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
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

                // O serviço retorna o DTO
                UsuarioDTO? usuarioLogado = await _authService.LoginAsync(email, senha);

                if (usuarioLogado != null)
                {
                    // --- GRAVA NA SESSÃO ---
                    // Agora funciona pois SessaoSistema espera um UsuarioDTO
                    SessaoSistema.UsuarioLogado = usuarioLogado;

                    // Abre o sistema principal
                    AbrirSistemaPrincipal();
                }
                else
                {
                    TxtErro.Text = "E-mail ou senha inválidos.";
                }
            }
            catch (Exception ex)
            {
                // Log para debug (opcional)
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                TxtErro.Text = "Erro de conexão ou dados incorretos.";
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