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
            .HasMaxLength(TaxonConstraints.PermalinkMaxLength); 
        
        builder.Property(x => x.Permalink)
            .IsRequired()
            .HasMaxLength(TaxonConstraints.PermalinkMaxLength);
        
        builder.HasIndex(x => x.Permalink).IsUnique();

        builder.Property(x => x.PrettyName)
            .IsRequired()
            .HasMaxLength(TaxonConstraints.PermalinkMaxLength);

        builder.Property(x => x.Automatic);
        builder.Property(x => x.RulesMatchPolicy)
            .HasMaxLength(10);
        builder.Property(x => x.SortOrder)
            .HasMaxLength(50);

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
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TaxonRules)
            .WithOne(x => x.Taxon)
            .HasForeignKey(x => x.TaxonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Classifications)
            .WithOne(x => x.Taxon)
            .HasForeignKey(x => x.TaxonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}