using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(DatabaseTables.Payments, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Currency)
            .HasMaxLength(PaymentConstraints.CurrencyMaxLength)
            .IsRequired();

        builder.Property(x => x.PaymentMethodType)
            .HasMaxLength(PaymentConstraints.PaymentMethodTypeMaxLength)
            .IsRequired();

        builder.Property(x => x.ReferenceTransactionId)
            .HasMaxLength(PaymentConstraints.ReferenceTransactionIdMaxLength);

        builder.Property(x => x.GatewayAuthCode)
            .HasMaxLength(PaymentConstraints.GatewayAuthCodeMaxLength);

        builder.Property(x => x.GatewayErrorCode)
            .HasMaxLength(PaymentConstraints.GatewayErrorCodeMaxLength);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(PaymentConstraints.FailureReasonMaxLength);

        builder.Property(x => x.PublicMetadata).HasColumnType("jsonb");
        builder.Property(x => x.PrivateMetadata).HasColumnType("jsonb");

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ReferenceTransactionId);
    }
}
