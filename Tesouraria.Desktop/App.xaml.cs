using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Mappings;
using Tesouraria.Application.Services;
using Tesouraria.Domain.Interfaces;
// CORREÇÃO AQUI: Usando Infrastructure
using Tesouraria.Infrastructure.Data;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Infra.Data.Repositories;
using Tesouraria.Infrastructure.Repositories;
using Tesouraria.Domain.Services;
using Tesouraria.Desktop.Views;
using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop
{
    public partial class App : System.Windows.Application
    {
        public IHost Host { get; private set; }

        public App()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // 1. Banco de Dados
                    string connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                                              ?? "Server=(localdb)\\mssqllocaldb;Database=TesourariaDb;Trusted_Connection=True;";

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlServer(connectionString);
                    });

                    // 2. Infraestrutura (Repositórios Genéricos)
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                    // 3. Aplicação (Serviços e AutoMapper)
                    services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));
                    // --- IMPORTANTE: REGISTRAR O SERVIÇO DE LOGIN ---
                    // Se você ainda não criou a classe concreta AuthService, o programa vai quebrar aqui.
                    // Assumindo que você tem a interface IAuthService e a classe AuthService:
                    services.AddScoped<IAuthService, AuthService>();
                    
                    services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

                    // 4. Interface Gráfica (Janelas)
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<LoginWindow>(); // <--- Registra a LoginWindow
                    services.AddTransient<IUsuarioRepository, UsuarioRepository>();
                    services.AddTransient<CadastroFielWindow>();
                    services.AddScoped<FielService>();
                    services.AddTransient<IFielRepository, FielRepository>();

                    services.AddTransient<FornecedorService>();
                    services.AddScoped<IFornecedorRepository, FornecedorRepository>();
                    services.AddTransient<CadastroFornecedor>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host.StartAsync();

            // --- INÍCIO DA SEED AUTOMÁTICA ---
            using (var scope = Host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Garante que o banco existe
                // context.Database.EnsureCreated(); // Opcional se usa Migrations

                // Se não houver nenhum usuário, cria o Admin
                if (!context.Usuarios.Any())
                {
                    context.Usuarios.Add(new Tesouraria.Domain.Entities.Usuario
                    {
                        Nome = "Administrador",
                        Email = "admin@paroquia.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Perfil = Domain.Enums.PerfilUsuario.Administrador,
                        Ativo = true,
                        DataCriacao = DateTime.Now
                    });
                    context.SaveChanges();
                }
            }
            // --- FIM DA SEED AUTOMÁTICA ---
            try
            {
                // CORREÇÃO: Chamamos a LoginWindow primeiro
                var loginWindow = Host.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro fatal ao iniciar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (Host)
            {
                await Host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}