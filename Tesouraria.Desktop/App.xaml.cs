using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using Tesouraria.Infrastructure.Data;
using Serilog;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Services;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Repositories;

namespace Tesouraria.Desktop
{
    public partial class App : System.Windows.Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext())
                .ConfigureServices((context, services) =>
                {
                    // Configuração do Banco de Dados
                    string connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                                              ?? throw new InvalidOperationException("Connection string not found.");

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    using (var scope = services.BuildServiceProvider().CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        // Garante que o banco existe
                        db.Database.EnsureCreated();

                        var admin = db.Usuarios.FirstOrDefault(u => u.Email == "admin@paroquia.com");
                        if (admin != null)
                        {
                            // Agora sim podemos usar a função dinâmica, pois estamos em tempo de execução, não criação de modelo
                            var hashCorreto = BCrypt.Net.BCrypt.HashPassword("admin123");

                            // Atualiza direto no banco via SQL cru para não precisar mudar a entidade agora
                            // Ou via EF (precisaria tornar o set da SenhaHash publico ou ter metodo)
                            // Vamos via EF usando o metodo que criamos:
                            admin.AlterarSenha(hashCorreto);
                            db.SaveChanges();
                        }
                    }
                    // Registro da Janela Principal
                    services.AddSingleton<MainWindow>();

                    // Registros do Domínio/Infra
                    services.AddScoped<IUsuarioRepository, UsuarioRepository>();

                    // Registros da Aplicação
                    services.AddScoped<IAuthService, AuthService>();

                    // Login Window (vamos criar em breve)
                    services.AddTransient<LoginWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            // AGORA PEDIMOS A TELA DE LOGIN PRIMEIRO
            var loginForm = AppHost.Services.GetRequiredService<LoginWindow>();
            loginForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}