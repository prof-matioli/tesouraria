using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Core;
using Tesouraria.Desktop.Views;

namespace Tesouraria.Desktop.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IServiceProvider _serviceProvider;

        // Propriedades de Interface
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email)); // Avisa a tela se mudar
                }
            }
        }

        private string _mensagemErro = string.Empty;
        public string MensagemErro
        {
            get => _mensagemErro;
            set => SetProperty(ref _mensagemErro, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(BotaoTexto));
                    OnPropertyChanged(nameof(CanLogin)); // Atualiza estado do botão
                }
            }
        }

        public string BotaoTexto => IsLoading ? "Verificando..." : "ENTRAR";
        public bool CanLogin => !IsLoading;

        // Evento para pedir à View que feche
        public event Action? RequestClose;

        // Comandos
        public ICommand EntrarCommand { get; }
        public ICommand SairCommand { get; }

        public LoginViewModel(IUsuarioService usuarioService, IServiceProvider serviceProvider)
        {
            _usuarioService = usuarioService;
            _serviceProvider = serviceProvider;

            // O comando recebe o PasswordBox como parâmetro
            EntrarCommand = new RelayCommand(async param => await RealizarLogin(param), _ => CanLogin);
            SairCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
        }

        private async Task RealizarLogin(object? parameter)
        {
            // Obtém a senha do PasswordBox (enviado via CommandParameter)
            var passwordBox = parameter as PasswordBox;
            var senha = passwordBox?.Password ?? string.Empty;

            // Validação básica
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(senha))
            {
                MensagemErro = "Por favor, preencha e-mail e senha.";
                return;
            }

            try
            {
                IsLoading = true;
                MensagemErro = string.Empty; // Limpa mensagens anteriores

                // Chama o serviço de autenticação
               // UsuarioDTO? usuario = await _authService.LoginAsync(Email.Trim(), senha);
                UsuarioDTO? usuario = await _usuarioService.AutenticarAsync(Email.Trim(), senha);
                if (usuario != null)
                {
                    // Login com sucesso: Salva na sessão e abre a tela principal
                    SessaoSistema.UsuarioLogado = usuario;
                    AbrirSistemaPrincipal();
                }
                else
                {
                    // Login falhou (credenciais inválidas)
                    MensagemErro = "E-mail ou senha inválidos.";
                }
            }
            catch (Exception)
            {
                // Erro técnico (ex: Banco de dados offline)
                MensagemErro = "Não foi possível conectar ao servidor. Tente novamente.";
            }
            finally
            {
                IsLoading = false; // Libera o botão
            }
        }
        private void AbrirSistemaPrincipal()
        {
            // Resolve e abre a janela principal
            var mainView = _serviceProvider.GetRequiredService<MainWindow>();
            mainView.Show();

            // Dispara evento para fechar a tela de login
            RequestClose?.Invoke();
        }
    }
}