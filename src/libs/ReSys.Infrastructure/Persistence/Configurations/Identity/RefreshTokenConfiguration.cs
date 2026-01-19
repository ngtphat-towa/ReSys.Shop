using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Shared.Constants;
using ReSys.Infrastructure.Persistence.Extensions;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", DatabaseSchemas.Identity);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(RefreshTokenConstraints.TokenHashMaxLength);

        builder.Property(t => t.CreatedByIp)
            .HasMaxLength(RefreshTokenConstraints.IpAddressMaxLength);

        builder.Property(t => t.RevokedByIp)
            .HasMaxLength(RefreshTokenConstraints.IpAddressMaxLength);

        builder.Property(t => t.RevokedReason)
            .HasMaxLength(RefreshTokenConstraints.RevokedReasonMaxLength);

        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditable();
    }
}