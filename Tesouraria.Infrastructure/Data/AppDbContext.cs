using Microsoft.EntityFrameworkCore;

namespace Tesouraria.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aqui adicionaremos os DbSets futuramente (ex: public DbSet<Usuario> Usuarios { get; set; })

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aplica configurações de mapeamento (Fluent API)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}