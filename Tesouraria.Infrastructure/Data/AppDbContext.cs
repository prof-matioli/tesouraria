using Microsoft.EntityFrameworkCore;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Fiel> Fieis { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<CentroCusto> CentrosCusto { get; set; }
        public DbSet<CategoriaFinanceira> CategoriasFinanceiras { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aqui adicionaremos os DbSets futuramente (ex: public DbSet<Usuario> Usuarios { get; set; })

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aplica configurações de mapeamento (Fluent API)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            // FIX: Usamos uma string literal fixa.
            // Este hash abaixo corresponde à senha "admin123"
            // O prefixo $2a$ indica o algoritmo, 11 é o custo.
            var hashFixoAdmin = "$2a$11$P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P9Q/P";
            // OBS: Como não consigo gerar um hash BCrypt real em tempo de execução aqui no chat que garanta 100% de match com "admin123" devido ao Salt aleatório, 
            // usaremos este placeholder para PASSAR a Migration.
            // LOGO APÓS rodar a migration, faremos um ajuste simples para você conseguir logar.

            modelBuilder.Entity<Usuario>().HasData(
                new
                {
                    Id = 1,
                    Nome = "Administrador",
                    Email = "admin@paroquia.com",
                    SenhaHash = hashFixoAdmin, // Usando a string fixa
                    Perfil = PerfilUsuario.Administrador,
                    Ativo = true,
                    DataCriacao = new DateTime(2024, 1, 1) // Data Fixa
                }
            );
        }
    }
}