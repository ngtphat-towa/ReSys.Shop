using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Shared.Constants;
using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Taxonomies;

public class TaxonConfiguration : IEntityTypeConfiguration<Taxon>
{
    public void Configure(EntityTypeBuilder<Taxon> builder)
    {
        builder.ToTable(DatabaseTables.Taxa, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(TaxonConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(TaxonConstraints.PresentationMaxLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(TaxonConstraints.PermalinkMaxLength); // Using Permalink Max Length for safety
        
        builder.Property(x => x.Permalink)
            .IsRequired()
            .HasMaxLength(TaxonConstraints.PermalinkMaxLength);
        
        builder.HasIndex(x => x.Permalink).IsUnique();

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(ProductConstraints.Seo.MetaTitleMaxLength);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(ProductConstraints.Seo.MetaDescriptionMaxLength);
        
        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(ProductConstraints.Seo.MetaKeywordsMaxLength);

        // Nested Set
        builder.Property(x => x.Lft);
        builder.Property(x => x.Rgt);
        builder.Property(x => x.Depth);

        // Relationships
        builder.HasOne(x => x.Parent)
            .WithMany(t => t.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of children to avoid orphans or cycles issues if DB doesn't support recursive cascade well (Postgres usually fine but safer to restrict)
            
        // Note: Taxonomy relationship is configured in TaxonomyConfiguration
    }
}
