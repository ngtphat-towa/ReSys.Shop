using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain;

namespace ReSys.Infrastructure.Persistence.Configurations;

public class ExampleEmbeddingConfiguration : IEntityTypeConfiguration<ExampleEmbedding>
{
    public void Configure(EntityTypeBuilder<ExampleEmbedding> builder)
    {
        // Primary Key
        builder.HasKey(e => e.ExampleId);

        // Properties
        builder.Property(e => e.Embedding)
            .HasColumnType("vector(384)");
        
        builder.HasOne(e => e.Example)
            .WithOne(p => p.Embedding)
            .HasForeignKey<ExampleEmbedding>(e => e.ExampleId);
    }
}
