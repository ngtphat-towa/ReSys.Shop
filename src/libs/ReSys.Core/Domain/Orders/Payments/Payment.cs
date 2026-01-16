using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Orders.Payments;

public sealed class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentState State { get; private set; } = PaymentState.Pending;
    public string? TransactionId { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount, string currency = "USD")
    {
        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Currency = currency,
            State = PaymentState.Pending
        };
    }

    public enum PaymentState
    {
        Pending,
        Completed,
        Failed,
        Voided,
        Refunded
    }
}
