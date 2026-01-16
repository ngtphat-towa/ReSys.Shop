using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Taxonomies;

public class TaxonomyConfiguration : IEntityTypeConfiguration<Taxonomy>
{
    public void Configure(EntityTypeBuilder<Taxonomy> builder)
    {
        builder.ToTable(DatabaseTables.Taxonomies, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(TaxonomyConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(TaxonomyConstraints.PresentationMaxLength);

        builder.Property(x => x.Position);

        // Relationships
        builder.HasMany(x => x.Taxons)
            .WithOne(t => t.Taxonomy)
            .HasForeignKey(t => t.TaxonomyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
