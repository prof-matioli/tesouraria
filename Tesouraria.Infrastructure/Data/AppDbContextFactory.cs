using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tesouraria.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Coloque aqui a MESMA string de conexão que está no seu App.xaml.cs ou appsettings.json
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Tesouraria;Trusted_Connection=True;");
            optionsBuilder.EnableSensitiveDataLogging(true);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}