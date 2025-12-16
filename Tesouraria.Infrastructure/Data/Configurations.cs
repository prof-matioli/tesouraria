using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Cria um índice único para não permitir dois usuários com mesmo email
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.SenhaHash)
                .IsRequired();

            // Salva o Enum como Texto no banco (fica mais legível: 'Administrador' em vez de '1')
            builder.Property(u => u.Perfil)
                .HasConversion<string>()
                .IsRequired();
        }
    }
}