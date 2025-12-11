using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using Tesouraria.Infrastructure.Data;
using Serilog;

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

                    // Registro da Janela Principal
                    services.AddSingleton<MainWindow>();

                    // Futuramente registraremos Serviços e Repositórios aqui
                    // services.AddScoped<IUsuarioRepository, UsuarioRepository>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            // Resolve a MainWindow do container de DI e exibe
            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}