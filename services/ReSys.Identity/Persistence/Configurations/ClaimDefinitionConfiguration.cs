using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence.Constants;

namespace ReSys.Identity.Persistence.Configurations;

public class ClaimDefinitionConfiguration : IEntityTypeConfiguration<ClaimDefinition>
{
    public void Configure(EntityTypeBuilder<ClaimDefinition> builder)
    {
        builder.ToTable(TableNames.ClaimDefinitions, Schemas.Identity);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(250);

        builder.Property(x => x.Category)
            .HasMaxLength(50)
            .HasDefaultValue("General");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.Type, x.Value }).IsUnique();
    }
}
