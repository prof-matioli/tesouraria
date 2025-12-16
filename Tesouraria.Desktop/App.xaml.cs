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
using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;
using Tesouraria.Desktop.ViewModels;

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

                    services.AddScoped<ICentroCustoRepository, CentroCustoRepository>();
                    services.AddScoped<IBaseService<CentroCusto, CentroCustoDTO>, CentroCustoService>();
                    services.AddTransient<CentroCustoWindow>();

                    services.AddScoped<ICategoriaFinanceiraRepository, CategoriaFinanceiraRepository>();
                    services.AddScoped<IBaseService<CategoriaFinanceira, CategoriaFinanceiraDTO>, CategoriaFinanceiraService>();
                    services.AddTransient<CategoriaWindow>();

                    services.AddScoped<ILancamentoRepository, LancamentoRepository>();
                    services.AddScoped<ILancamentoService, LancamentoService>();

                    // --- FASE 4: REGISTROS FINANCEIROS ---

                    // 1. Repositórios
                    services.AddScoped<ILancamentoRepository, LancamentoRepository>();
                    services.AddScoped<IRepository<CentroCusto>, Repository<CentroCusto>>();
                    services.AddScoped<IRepository<CategoriaFinanceira>, Repository<CategoriaFinanceira>>();
                    services.AddScoped<IRepository<Fiel>, Repository<Fiel>>();
                    services.AddScoped<IRepository<Fornecedor>, Repository<Fornecedor>>();

                    // 2. Serviços
                    services.AddScoped<ILancamentoService, LancamentoService>();

                    // 3. ViewModels
                    services.AddTransient<LancamentoListaViewModel>();
                    services.AddTransient<LancamentoCadastroViewModel>();

                    // 4. Views (Janelas)
                    services.AddTransient<LancamentoListaView>();
                    services.AddTransient<LancamentoCadastroView>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Inicia o Host (DI)
            await Host.StartAsync();

            // --- INÍCIO DA SEED AUTOMÁTICA ---
            using (var scope = Host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Garante que o banco está atualizado com as últimas Migrations
                // context.Database.Migrate(); 

                // 1. Seed de Usuário (Mantido do seu código original)
                if (!context.Usuarios.Any())
                {
                    context.Usuarios.Add(new Tesouraria.Domain.Entities.Usuario
                    {
                        Nome = "Administrador",
                        Email = "admin@paroquia.com",
                        SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                        Perfil = Tesouraria.Domain.Enums.PerfilUsuario.Administrador,
                        Ativo = true,
                        DataCriacao = DateTime.Now
                    });
                    await context.SaveChangesAsync();
                }

                // 2. Seed Básico Financeiro (NOVO - Para testar a Fase 4)
                // Precisamos disso para preencher os Combos na tela de cadastro
                if (!context.CentrosCusto.Any())
                {
                    context.CentrosCusto.AddRange(
                        new Tesouraria.Domain.Entities.CentroCusto { Nome = "Geral", Descricao = "Custos gerais da paróquia" },
                        new Tesouraria.Domain.Entities.CentroCusto { Nome = "Festas", Descricao = "Eventos festivos" },
                        new Tesouraria.Domain.Entities.CentroCusto { Nome = "Liturgia", Descricao = "Custos com missas e celebrações" }
                    );
                    await context.SaveChangesAsync();
                }

                if (!context.CategoriasFinanceiras.Any())
                {
                    context.CategoriasFinanceiras.AddRange(
                        // Receitas
                        new Tesouraria.Domain.Entities.CategoriaFinanceira { Nome = "Dízimo", Tipo = Tesouraria.Domain.Entities.TipoTransacao.Receita, DedutivelIR = true },
                        new Tesouraria.Domain.Entities.CategoriaFinanceira { Nome = "Oferta", Tipo = Tesouraria.Domain.Entities.TipoTransacao.Receita, DedutivelIR = false },
                        // Despesas
                        new Tesouraria.Domain.Entities.CategoriaFinanceira { Nome = "Energia Elétrica", Tipo = Tesouraria.Domain.Entities.TipoTransacao.Despesa },
                        new Tesouraria.Domain.Entities.CategoriaFinanceira { Nome = "Manutenção Predial", Tipo = Tesouraria.Domain.Entities.TipoTransacao.Despesa }
                    );
                    await context.SaveChangesAsync();
                }
            }
            // --- FIM DA SEED AUTOMÁTICA ---

            try
            {
                //Comentado para uso futuro:
                var loginWindow = Host.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro fatal ao iniciar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                // Opcional: Shutdown se falhar
                Current.Shutdown();
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