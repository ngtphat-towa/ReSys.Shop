using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Taxonomies;

public class TaxonRuleConfiguration : IEntityTypeConfiguration<TaxonRule>
{
    public void Configure(EntityTypeBuilder<TaxonRule> builder)
    {
        builder.ToTable(DatabaseTables.TaxonRules, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(TaxonRuleConstraints.TypeMaxLength);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(TaxonRuleConstraints.ValueMaxLength);

        builder.Property(x => x.MatchPolicy)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PropertyName)
            .HasMaxLength(TaxonRuleConstraints.PropertyNameMaxLength);

        builder.HasOne(x => x.Taxon)
            .WithMany(x => x.TaxonRules)
            .HasForeignKey(x => x.TaxonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
