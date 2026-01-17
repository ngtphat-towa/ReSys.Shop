using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Testing.Examples;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Testing.Examples;

public class ExampleEmbeddingConfiguration : IEntityTypeConfiguration<ExampleEmbedding>
{
    public void Configure(EntityTypeBuilder<ExampleEmbedding> builder)
    {
        builder.ToTable(DatabaseTables.ExampleEmbeddings, DatabaseSchemas.Testing);

        builder.HasKey(e => e.ExampleId);
        builder.Property(e => e.Embedding).HasColumnType("vector(384)");
        
        builder.HasOne(e => e.Example)
            .WithOne(p => p.Embedding)
            .HasForeignKey<ExampleEmbedding>(e => e.ExampleId);
    }
}
