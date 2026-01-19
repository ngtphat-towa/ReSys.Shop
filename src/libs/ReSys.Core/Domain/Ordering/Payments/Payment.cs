using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Payments;

/// <summary>
/// Represents a financial transaction against an order.
/// Orchestrates the lifecycle from authorization to capture and potential refund.
/// </summary>
public sealed class Payment : Aggregate, IHasMetadata
{
    public enum PaymentState
    {
        Pending = 0,
        Authorizing = 1,
        Authorized = 2,
        Capturing = 3,
        Completed = 4,
        Refunded = 5,
        Failed = 7,
        Void = 8,
        RequiresAction = 9
    }

    #region Properties
    public Guid OrderId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public long AmountCents { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentState State { get; set; } = PaymentState.Pending;
    public string PaymentMethodType { get; set; } = string.Empty;

    public string? ReferenceTransactionId { get; set; }
    public string? GatewayAuthCode { get; set; }
    public string? GatewayErrorCode { get; set; }

    public DateTimeOffset? AuthorizedAt { get; set; }
    public DateTimeOffset? CapturedAt { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Total amount refunded in cents. Tracks cumulative partial refunds.
    /// </summary>
    public long RefundedAmountCents { get; set; }

    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
    #endregion

    public Payment() { }

    #region Factory Methods
    /// <summary>
    /// Factory for creating a new payment record.
    /// </summary>
    public static ErrorOr<Payment> Create(
        Guid orderId,
        long amountCents,
        string currency,
        string paymentMethodType,
        Guid? paymentMethodId = null,
        string? idempotencyKey = null)
    {
        if (amountCents < PaymentConstraints.AmountCentsMinValue) return PaymentErrors.InvalidAmountCents;
        if (string.IsNullOrWhiteSpace(currency)) return PaymentErrors.CurrencyRequired;
        if (string.IsNullOrWhiteSpace(paymentMethodType)) return PaymentErrors.PaymentMethodTypeRequired;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            PaymentMethodId = paymentMethodId,
            AmountCents = amountCents,
            Currency = currency.ToUpperInvariant(),
            State = PaymentState.Pending,
            PaymentMethodType = paymentMethodType,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTimeOffset.UtcNow
        };

        payment.RaiseDomainEvent(new PaymentEvents.PaymentCreated(payment));
        return payment;
    }
    #endregion

    #region Business Logic

    public ErrorOr<Success> MarkAsAuthorized(string transactionId, string? gatewayAuthCode = null)
    {
        if (State == PaymentState.Authorized) return Result.Success;

        if (State != PaymentState.Pending && State != PaymentState.Authorizing && State != PaymentState.RequiresAction)
            return PaymentErrors.InvalidStateTransition(State, PaymentState.Authorized);

        ReferenceTransactionId = transactionId;
        GatewayAuthCode = gatewayAuthCode;
        State = PaymentState.Authorized;
        AuthorizedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentEvents.PaymentAuthorized(this));
        return Result.Success;
    }

    public ErrorOr<Success> MarkAsCaptured(string? transactionId = null)
    {
        if (State == PaymentState.Completed) return Result.Success;

        if (State != PaymentState.Authorized && State != PaymentState.Pending)
            return PaymentErrors.InvalidStateTransition(State, PaymentState.Completed);

        ReferenceTransactionId = transactionId ?? ReferenceTransactionId;

        if (string.IsNullOrEmpty(ReferenceTransactionId))
            return PaymentErrors.ReferenceTransactionIdRequired;

        State = PaymentState.Completed;
        CapturedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentEvents.PaymentCaptured(this));
        return Result.Success;
    }

    public ErrorOr<Success> Void()
    {
        if (State == PaymentState.Void) return Result.Success;
        if (State == PaymentState.Completed) return PaymentErrors.CannotVoidCaptured;

        if (State != PaymentState.Authorized && State != PaymentState.Pending)
            return PaymentErrors.InvalidStateTransition(State, PaymentState.Void);

        State = PaymentState.Void;
        VoidedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentEvents.PaymentVoided(this));
        return Result.Success;
    }

    public ErrorOr<Success> MarkAsFailed(string errorMessage, string? gatewayErrorCode = null)
    {
        if (State == PaymentState.Failed) return Result.Success;

        State = PaymentState.Failed;
        FailureReason = errorMessage;
        GatewayErrorCode = gatewayErrorCode;

        RaiseDomainEvent(new PaymentEvents.PaymentFailed(this, errorMessage));
        return Result.Success;
    }

    public ErrorOr<Success> Refund(long amountCents, string reason, string? referenceTransactionId = null)
    {
        if (amountCents <= 0) return PaymentErrors.InvalidAmountCents;

        if (State != PaymentState.Completed && State != PaymentState.Refunded)
            return PaymentErrors.InvalidStateTransition(State, PaymentState.Refunded);

        var remainingBalance = AmountCents - RefundedAmountCents;
        if (amountCents > remainingBalance)
            return PaymentErrors.RefundExceedsBalance(amountCents / 100m, remainingBalance / 100m);

        RefundedAmountCents += amountCents;

        if (RefundedAmountCents >= AmountCents)
        {
            State = PaymentState.Refunded;
        }

        ReferenceTransactionId = referenceTransactionId ?? ReferenceTransactionId;

        RaiseDomainEvent(new PaymentEvents.PaymentRefunded(this, amountCents));
        return Result.Success;
    }

    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }

    #endregion
}
