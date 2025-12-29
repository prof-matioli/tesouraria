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
                .HasMaxLength(150);

            // Índice único (Perfeito!)
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.SenhaHash)
                .IsRequired();

            builder.HasOne(u => u.Perfil)     // Um Usuário tem UM Perfil
                   .WithMany()                // Um Perfil tem MUITOS Usuários
                   .HasForeignKey(u => u.PerfilId) // A chave que liga é PerfilId
                   .IsRequired();             // É obrigatório ter perfil
        }
    }
}