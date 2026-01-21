using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Ordering.Payments;

public static class PaymentEvents
{
    public record PaymentCreated(Payment Payment) : IDomainEvent;
    public record PaymentAuthorized(Payment Payment) : IDomainEvent;
    public record PaymentCaptured(Payment Payment) : IDomainEvent;
    public record PaymentRefunded(Payment Payment, long RefundedAmount) : IDomainEvent;
    public record PaymentFailed(Payment Payment, string Reason) : IDomainEvent;
    public record PaymentVoided(Payment Payment) : IDomainEvent;

    // Webhook Events
    public record ExternalPaymentCaptured(Guid OrderId, Guid PaymentId, string TransactionId, long AmountCents, DateTimeOffset OccurredAt) : IDomainEvent;
}
