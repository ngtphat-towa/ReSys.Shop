using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Settings.PaymentMethods;

using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Payments;

/// <summary>
/// Represents a financial transaction against an order.
/// Orchestrates the lifecycle from authorization to capture and potential refund.
/// It acts as the bridge between the domain and external gateways (Stripe, PayPal).
/// </summary>
public sealed class Payment : Aggregate, IHasMetadata
{
    /// <summary>
    /// Financial states of a payment transaction.
    /// </summary>
    public enum PaymentState
    {
        /// <summary>Initialized but not processed.</summary>
        Pending = 0,
        /// <summary>In communication with gateway.</summary>
        Authorizing = 1,
        /// <summary>Funds reserved but not transferred.</summary>
        Authorized = 2,
        /// <summary>Funds transfer in progress.</summary>
        Capturing = 3,
        /// <summary>Terminal Success: Funds received.</summary>
        Completed = 4,
        /// <summary>Funds returned to customer.</summary>
        Refunded = 5,
        /// <summary>Terminal Error: Transaction rejected.</summary>
        Failed = 7,
        /// <summary>Authorization cancelled before capture.</summary>
        Void = 8,
        /// <summary>Awaiting 3D Secure or additional user input.</summary>
        RequiresAction = 9
    }

    #region Properties
    /// <summary>Parent order reference.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Reference to the provider configuration.</summary>
    public Guid? PaymentMethodId { get; set; }

    /// <summary>Navigation to provider config for secret retrieval.</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>Value of the transaction in minor units (cents).</summary>
    public long AmountCents { get; set; }

    /// <summary>ISO 4217 Currency code.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Current lifecycle status.</summary>
    public PaymentState State { get; set; } = PaymentState.Pending;

    /// <summary>Provider identifier (e.g. Stripe).</summary>
    public string PaymentMethodType { get; set; } = string.Empty;

    /// <summary>External gateway transaction/intent ID.</summary>
    public string? ReferenceTransactionId { get; set; }

    /// <summary>Authorization code provided by gateway.</summary>
    public string? GatewayAuthCode { get; set; }

    /// <summary>Machine-readable error code from provider.</summary>
    public string? GatewayErrorCode { get; set; }

    /// <summary>Timestamp of fund reservation.</summary>
    public DateTimeOffset? AuthorizedAt { get; set; }

    /// <summary>Timestamp of fund receipt.</summary>
    public DateTimeOffset? CapturedAt { get; set; }

    /// <summary>Timestamp of void/cancel.</summary>
    public DateTimeOffset? VoidedAt { get; set; }

    /// <summary>Descriptive reason for transaction failure.</summary>
    public string? FailureReason { get; set; }

    /// <summary>Safety token to prevent duplicate charges.</summary>
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

    /// <summary>
    /// Confirms that funds have been successfully reserved at the gateway.
    /// This is typically called after a successful 'Authorize' call or a Webhook intent.
    /// </summary>
    public ErrorOr<Success> MarkAsAuthorized(string transactionId, string? gatewayAuthCode = null)
    {
        // Guard: Prevent redundant updates if already in target state.
        if (State == PaymentState.Authorized) return Result.Success;

        // Guard: Enforce valid state flow (cannot authorize a failed or voided payment).
        if (State != PaymentState.Pending && State != PaymentState.Authorizing && State != PaymentState.RequiresAction)
            return PaymentErrors.InvalidStateTransition(State, PaymentState.Authorized);

        // Snapshot: Link the internal record to the external gateway reference.
        ReferenceTransactionId = transactionId;
        GatewayAuthCode = gatewayAuthCode;
        State = PaymentState.Authorized;
        AuthorizedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentEvents.PaymentAuthorized(this));
        return Result.Success;
    }

    /// <summary>
    /// Confirms the actual transfer of funds. 
    /// This is the 'Point of No Return' for the customer's money.
    /// </summary>
    public ErrorOr<Success> MarkAsCaptured(string? transactionId = null)
    {
        if (State == PaymentState.Completed) return Result.Success;

        // Guard: We allow capture from Pending (Immediate Pay) or Authorized (Reservation).
        if (State != PaymentState.Authorized && State != PaymentState.Pending)
            return PaymentErrors.InvalidStateTransition(State, PaymentState.Completed);

        ReferenceTransactionId = transactionId ?? ReferenceTransactionId;

        // Invariant: Cannot record a completion without a traceable ID.
        if (string.IsNullOrEmpty(ReferenceTransactionId))
            return PaymentErrors.ReferenceTransactionIdRequired;

        State = PaymentState.Completed;
        CapturedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentEvents.PaymentCaptured(this));
        return Result.Success;
    }

    /// <summary>
    /// Releases the fund reservation at the gateway. 
    /// Used when an order is canceled before money is actually moved.
    /// </summary>
    public ErrorOr<Success> Void()
    {
        if (State == PaymentState.Void) return Result.Success;
        
        // Guard: Once money is captured, it must be 'Refunded', not 'Voided'.
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
