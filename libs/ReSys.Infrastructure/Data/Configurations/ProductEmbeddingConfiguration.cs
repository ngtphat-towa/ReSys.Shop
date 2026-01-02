using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Entities;

namespace ReSys.Infrastructure.Data.Configurations;

public class ProductEmbeddingConfiguration : IEntityTypeConfiguration<ProductEmbedding>
{
    public void Configure(EntityTypeBuilder<ProductEmbedding> builder)
    {
        builder.HasKey(e => e.ProductId);
        builder.Property(e => e.Embedding).HasColumnType("vector(384)");
        
        builder.HasOne(e => e.Product)
            .WithOne(p => p.Embedding)
            .HasForeignKey<ProductEmbedding>(e => e.ProductId);
    }
}
