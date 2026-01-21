using ErrorOr;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Core.Domain.Ordering.Payments;

namespace ReSys.Core.Domain.Ordering.Payments.Gateways;

/// <summary>
/// Defines the contract for external payment gateway integrations.
/// Implementations reside in Infrastructure but are driven by Core entities.
/// </summary>
public interface IPaymentProcessor
{
    /// <summary>
    /// Gets the specific payment method type this processor handles (e.g., Stripe, PayPal).
    /// </summary>
    PaymentMethod.PaymentType Type { get; }

    /// <summary>
    /// Initiates a payment transaction with the external provider.
    /// </summary>
    /// <param name="settings">The configuration entity containing provider secrets (in PrivateMetadata).</param>
    /// <param name="payment">The domain payment record to be processed.</param>
    /// <param name="amountCents">The amount to charge in minor units (cents).</param>
    /// <param name="currency">The 3-letter ISO currency code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A PaymentResult containing the provider's transaction ID and status.</returns>
    Task<ErrorOr<PaymentResult>> ProcessPaymentAsync(
        PaymentMethod settings,
        Payment payment,
        long amountCents,
        string currency,
        CancellationToken ct = default);

    /// <summary>
    /// Validates and processes an incoming webhook payload.
    /// </summary>
    /// <param name="settings">The configuration entity containing provider secrets (in PrivateMetadata).</param>
    /// <param name="json">Raw JSON body from the provider.</param>
    /// <param name="signature">Provider signature header for verification.</param>
    /// <returns>Success if handled, Error if invalid.</returns>
    Task<ErrorOr<Success>> ProcessWebhookAsync(
        PaymentMethod settings,
        string json, 
        string signature);

    /// <summary>
    /// Refunds a captured payment.
    /// </summary>
    /// <param name="settings">The configuration entity containing provider secrets.</param>
    /// <param name="payment">The payment to refund.</param>
    /// <param name="amountCents">Amount to refund (defaults to full amount if equal to payment amount).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if refund initiated/completed.</returns>
    Task<ErrorOr<Success>> RefundAsync(
        PaymentMethod settings,
        Payment payment,
        long amountCents,
        CancellationToken ct = default);
}

/// <summary>
/// Represents the standardized result from any payment gateway.
/// </summary>
public record PaymentResult(string TransactionId, string Status, bool IsSuccess, string? ClientSecret = null);
