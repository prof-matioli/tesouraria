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
            optionsBuilder.UseSqlServer(@"Server=192.168.101.179; Database=Tesouraria; User Id=sa; Password=Senh@Forte; Integrated Security=SSPI;");
            optionsBuilder.EnableSensitiveDataLogging(true);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}