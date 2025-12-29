using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tesouraria.Application.Interfaces;
using Tesouraria.Application.Services;
using Tesouraria.Domain.Interfaces;
using Tesouraria.Infrastructure.Data;

// IMPORTANTE: Ajuste estes usings conforme a estrutura das suas pastas
using Tesouraria.Desktop.ViewModels;
using Tesouraria.Desktop.Views;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Desktop.Views.Relatorios;
using Tesouraria.Infrastructure.Repositories;
using Tesouraria.Infrastructure.Data.Repositories;

using Tesouraria.Application.Mappings;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;

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
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Configura a licença para uso Gratuito (Comunidade/Non-profit)
            QuestPDF.Settings.License = LicenseType.Community;
            // ============================================================

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                // 1. Configura a coleção de serviços
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                // 2. Constrói o provedor
                ServiceProvider = serviceCollection.BuildServiceProvider();

                // --- BLOCO DE CRIAÇÃO/RESET COM DEBUG ---
                try
                {
                    // Tenta criar o banco. Se der erro, vai cair no 'catch' e mostrar a mensagem.
                    ResetarBancoDados(ServiceProvider);

                }
                catch (Exception ex)
                {
                    // ESTA MENSAGEM VAI NOS DIZER O PROBLEMA REAL
                    MessageBox.Show($"ERRO FATAL AO CRIAR BANCO:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                                    "Erro de Inicialização",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                    // Fecha o app pois sem banco não funciona
                    Shutdown();
                    return;
                }
                // 3. Configura licença do QuestPDF
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                // 4. Inicia pela tela de Login
                // Envolvemos em um bloco try/catch para pegar erros de injeção
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();

                loginWindow.Show();
            }
            catch (Exception ex)
            {
                // Esse MessageBox vai te mostrar exatamente o que está impedindo o app de abrir
                string erro = $"Erro fatal na inicialização:\n\n{ex.Message}";

                if (ex.InnerException != null)
                    erro += $"\n\nDetalhe: {ex.InnerException.Message}";

                MessageBox.Show(erro, "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fecha o app para não ficar rodando fantasma
                Shutdown();
            }
        }

        private void ResetarBancoDados(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Tenta aplicar as migrations. 
                // Se o banco não existir, este comando CRIA o banco automaticamente.
                //dbContext.Database.Migrate();
                DbSeed.Seed(dbContext);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddLogging();
            // --- 1. BANCO DE DADOS ---
            services.AddDbContext<AppDbContext>(options =>
            {
                //options.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Tesouraria;Integrated Security=True;TrustServerCertificate=True");
                options.UseSqlServer(connectionString);
            });

            // --- 2. REPOSITÓRIOS ---
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ILancamentoRepository, LancamentoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            // --- 3. AUTO MAPPER ---
            // Corrigido para usar o método correto que aceita um tipo
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // --- 4. SERVIÇOS DE APLICAÇÃO ---
            services.AddScoped<ILancamentoService, LancamentoService>();
            services.AddTransient<IUsuarioService, UsuarioService>();

            // --- 5. VIEWMODELS ---
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LancamentoListaViewModel>();
            services.AddTransient<LancamentoCadastroViewModel>();
            services.AddTransient<RelatorioViewModel>();
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
            services.AddTransient<CadastroCategoriaFinanceiraWindow>();
            services.AddTransient<CadastroFielWindow>();     // Lista
            services.AddTransient<CadastroFielFormWindow>(); // Formulário
            services.AddTransient<CadastroFornecedorWindow>();
            services.AddTransient<CadastroFornecedorFormWindow>();
            services.AddTransient<CadastroCentroCustoWindow>();
            services.AddTransient<CadastroCentroCustoFormWindow>();
            services.AddTransient<CadastroCategoriaFinanceiraWindow>();
            services.AddTransient<CadastroCategoriaFinanceiraFormWindow>();
            services.AddTransient<CadastroUsuarioFormWindow>();
            services.AddTransient<CadastroUsuarioWindow>();
        }
    }
}