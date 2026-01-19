using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Infrastructure.Persistence.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ConfigureAuditable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable
    {
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        
        return builder;
    }

    public static EntityTypeBuilder<TEntity> ConfigureMetadata<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IHasMetadata
    {
        // Note: Actual converter is applied globally in ModelBuilderExtensions
        // This method can be used for entity-specific overrides if needed
        builder.Property(e => e.PublicMetadata).HasColumnType("jsonb");
        builder.Property(e => e.PrivateMetadata).HasColumnType("jsonb");

        return builder;
    }
}