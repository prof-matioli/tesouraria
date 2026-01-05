using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Services;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;
using Tesouraria.Desktop.ViewModels;
using Tesouraria.Desktop.Views;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Desktop.Views.Relatorios;
using Tesouraria.Desktop.Views.Ferramentas;
using Tesouraria.Infrastructure.Repositories;
using Tesouraria.Infrastructure.Data.Repositories;
using Tesouraria.Application.Mappings;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;
using Tesouraria.Desktop.Services; // Namespace do RelatorioPdfService

namespace Tesouraria.Desktop
{
    public partial class App : System.Windows.Application
    {
        // Propriedade pública para acesso ao Container de Injeção de Dependência
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public App()
        {
            // 1. CONSTRÓI A CONFIGURAÇÃO (Lê o JSON)
            // Isso garante que 'Configuration' esteja preenchido antes de qualquer coisa
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 2. Configura a licença do QuestPDF (Community)
                QuestPDF.Settings.License = LicenseType.Community;

                // 3. Configura a coleção de serviços (Injeção de Dependência)
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                // 4. Constrói o provedor
                ServiceProvider = serviceCollection.BuildServiceProvider();

                // ============================================================
                // CRIAÇÃO AUTOMÁTICA DO BANCO E TABELAS
                // ============================================================
                try
                {
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // Este comando cria o arquivo .db se não existir
                        // E cria todas as tabelas (Perfis, Usuarios, Lancamentos...)
                        dbContext.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criar banco de dados: {ex.Message}");
                    // Opcional: Shutdown se o banco for vital
                    // Current.Shutdown(); 
                    // return;
                }
                // ============================================================

                // 5. Inicialização do Banco de Dados (Seed/Migration)
                try
                {
                    ResetarBancoDados(ServiceProvider);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ERRO FATAL AO CONECTAR/CRIAR BANCO:\n\n{ex.Message}\n\nDetalhe: {ex.InnerException?.Message}",
                                    "Erro de Banco de Dados",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

                // 6. Abre a Janela de Login
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                string erro = $"Erro fatal na inicialização do sistema:\n\n{ex.Message}";
                if (ex.InnerException != null)
                    erro += $"\n\nDetalhe: {ex.InnerException.Message}";

                MessageBox.Show(erro, "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ResetarBancoDados(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // dbContext.Database.Migrate(); // Descomente se usar Migrations
                DbSeed.Seed(dbContext);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // --- 0. CONFIGURAÇÃO (ESSENCIAL PARA O RelatorioPdfService) ---
            // Registra a instância de Configuration criada no construtor para ser injetada onde precisar
            services.AddSingleton<IConfiguration>(Configuration);

            // --- 1. BANCO DE DADOS ---
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
            {
                //options.UseSqlServer(connectionString);
                options.UseSqlite(connectionString);
            }, ServiceLifetime.Transient);

            // --- 2. REPOSITÓRIOS ---
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ILancamentoRepository, LancamentoRepository>();
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<ICentroCustoRepository, CentroCustoRepository>(); // Caso tenha criado repositório específico
            services.AddTransient<IFielRepository, FielRepository>(); // Caso tenha criado repositório específico

            // --- 3. AUTO MAPPER ---
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // --- 4. SERVIÇOS DE APLICAÇÃO ---
            services.AddTransient<ILancamentoService, LancamentoService>();
            services.AddTransient<IUsuarioService, UsuarioService>();

            services.AddTransient<RelatorioPdfService>();
            services.AddTransient<ExtratoService>();

            // --- 5. VIEWMODELS ---
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LancamentoListaViewModel>();
            services.AddTransient<LancamentoCadastroViewModel>();
            services.AddTransient<DashboardViewModel>();

            services.AddTransient<RelatorioViewModel>();
            services.AddTransient<ImportacaoExtratoViewModel>();

            services.AddTransient<FielListaViewModel>();
            services.AddTransient<FielCadastroViewModel>();
            services.AddTransient<FornecedorListaViewModel>();
            services.AddTransient<FornecedorCadastroViewModel>();
            services.AddTransient<CentroCustoListaViewModel>();
            services.AddTransient<CentroCustoCadastroViewModel>();
            services.AddTransient<CategoriaFinanceiraCadastroViewModel>();
            services.AddTransient<CategoriaFinanceiraListaViewModel>();
            services.AddTransient<UsuarioCadastroViewModel>();
            services.AddTransient<UsuarioListaViewModel>();

            // --- 6. VIEWS ---
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<LancamentoListaView>();
            services.AddTransient<LancamentoCadastroView>();
            services.AddTransient<RelatorioView>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<CategoriaFinanceiraListaView>();
            services.AddTransient<FielListaView>();
            services.AddTransient<CadastroFielFormWindow>();
            services.AddTransient<FornecedorListaView>();
            services.AddTransient<CadastroFornecedorFormWindow>();
            services.AddTransient<CentroCustoListaView>();
            services.AddTransient<CadastroCentroCustoFormWindow>();
            services.AddTransient<CadastroCategoriaFinanceiraFormWindow>();
            services.AddTransient<UsuarioCadastroView>();
            services.AddTransient<UsuarioListaView>();
            services.AddTransient<ImportacaoExtratoView>();
        }
    }
}