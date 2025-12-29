using Microsoft.EntityFrameworkCore;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Fiel> Fieis { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<CentroCusto> CentrosCusto { get; set; }
        public DbSet<CategoriaFinanceira> CategoriasFinanceiras { get; set; }
        public DbSet<Lancamento> Lancamento { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aqui adicionaremos os DbSets futuramente (ex: public DbSet<Usuario> Usuarios { get; set; })
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aplica configurações de mapeamento (Fluent API)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            modelBuilder.Entity<Fornecedor>().HasQueryFilter(p => p.Ativo);
            modelBuilder.Entity<Fiel>().HasQueryFilter(p => p.Ativo);
            modelBuilder.Entity<CentroCusto>().HasQueryFilter(p => p.Ativo);
            modelBuilder.Entity<CategoriaFinanceira>().HasQueryFilter(p => p.Ativo);
            modelBuilder.Entity<Usuario>().HasQueryFilter(p => p.Ativo);
        }
    }
}