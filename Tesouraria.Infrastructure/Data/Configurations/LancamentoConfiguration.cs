using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Infrastructure.Data.Configurations
{
    public class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
    {
        public void Configure(EntityTypeBuilder<Lancamento> builder)
        {
            builder.ToTable("Lancamentos");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Descricao).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Observacao).HasMaxLength(500);

            // Mapeamento preciso para moeda
            builder.Property(x => x.ValorOriginal).HasPrecision(18, 2);
            builder.Property(x => x.ValorPago).HasPrecision(18, 2);

            // Relacionamentos
            builder.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CentroCusto).WithMany().HasForeignKey(x => x.CentroCustoId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Fiel).WithMany().HasForeignKey(x => x.FielId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Fornecedor).WithMany().HasForeignKey(x => x.FornecedorId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}