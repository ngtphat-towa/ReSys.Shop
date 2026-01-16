using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Settings;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Settings;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable(DatabaseTables.Settings, DatabaseSchemas.System);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(SettingConstraints.KeyMaxLength);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(SettingConstraints.ValueMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(SettingConstraints.DescriptionMaxLength);

        builder.Property(x => x.DefaultValue)
            .HasMaxLength(SettingConstraints.ValueMaxLength);

        builder.Property(x => x.ValueType)
            .HasConversion<string>();

        builder.HasIndex(x => x.Key).IsUnique();
        
        // Metadata is handled globally in AppDbContext via ApplyMetadataConfiguration
    }
}