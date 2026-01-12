using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain;
using ReSys.Core.Common.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations;

public class ExampleEmbeddingConfiguration : IEntityTypeConfiguration<ExampleEmbedding>
{
    public void Configure(EntityTypeBuilder<ExampleEmbedding> builder)
    {
        builder.ToTable(TableNames.ExampleEmbeddings);
        builder.HasKey(e => e.ExampleId);
        builder.Property(e => e.Embedding).HasColumnType("vector(384)");
        
        builder.HasOne(e => e.Example)
            .WithOne(p => p.Embedding)
            .HasForeignKey<ExampleEmbedding>(e => e.ExampleId);
    }
}
