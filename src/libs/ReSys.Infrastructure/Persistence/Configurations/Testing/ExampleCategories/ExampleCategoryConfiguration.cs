using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Testing.ExampleCategories;

namespace ReSys.Infrastructure.Persistence.Configurations.Testing.ExampleCategories;

public class ExampleCategoryConfiguration : IEntityTypeConfiguration<ExampleCategory>
{
    public void Configure(EntityTypeBuilder<ExampleCategory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(ExampleCategoryConstraints.NameMaxLength);

        builder.Property(e => e.Description)
            .HasMaxLength(ExampleCategoryConstraints.DescriptionMaxLength);

        builder.HasIndex(e => e.Name).IsUnique();

        // Relationship: One category has many examples
        builder.HasMany(e => e.Examples)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
