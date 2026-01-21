using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Domain.Ordering.Payments.Gateways;
using DomainPaymentMethod = ReSys.Core.Domain.Settings.PaymentMethods.PaymentMethod;
using Stripe;

namespace ReSys.Infrastructure.Payments.Gateways;

/// <summary>
/// Stripe implementation of the payment processor.
/// Connects to Stripe API to handle credit card transactions and webhooks.
/// </summary>
public sealed class StripeProcessor(IPublisher mediator, ILogger<StripeProcessor> logger) : IPaymentProcessor
{
    public DomainPaymentMethod.PaymentType Type => DomainPaymentMethod.PaymentType.Stripe;

    /// <inheritdoc />
    public async Task<ErrorOr<PaymentResult>> ProcessPaymentAsync(
        DomainPaymentMethod settings,
        Payment payment,
        long amountCents,
        string currency,
        CancellationToken ct = default)
    {
        // Guard: Validate configuration existence
        if (!settings.PrivateMetadata.TryGetValue("SecretKey", out var secretObj) || secretObj is not string secretKey)
        {
            logger.LogError("Stripe SecretKey missing for PaymentMethod {Id}", settings.Id);
            return Error.Failure("Stripe.ConfigurationMissing", "Stripe Secret Key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(secretKey))
            return Error.Failure("Stripe.InvalidKey", "Stripe Secret Key is empty.");

        try
        {
            // Business Rule: Initialize Stripe client dynamically using the entity's secret.
            var client = new StripeClient(secretKey);
            var service = new PaymentIntentService(client);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountCents,
                Currency = currency.ToLowerInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    ["OrderReference"] = payment.OrderId.ToString(), // Links to Aggregate Root
                    ["PaymentReference"] = payment.Id.ToString(),    // Links to Entity
                    ["Platform"] = "ReSys.Shop"
                }
            };

            var intent = await service.CreateAsync(options, cancellationToken: ct);

            return new PaymentResult(
                TransactionId: intent.Id,
                Status: intent.Status,
                IsSuccess: intent.Status == "succeeded" || intent.Status == "requires_capture" || intent.Status == "processing",
                ClientSecret: intent.ClientSecret
            );
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe API error for Payment {Id}", payment.Id);
            return Error.Failure("Stripe.GatewayError", ex.StripeError.Message ?? "Unknown Stripe error");
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> ProcessWebhookAsync(
        DomainPaymentMethod settings,
        string json,
        string signature)
    {
        // Guard: Get Webhook Secret
        if (!settings.PrivateMetadata.TryGetValue("WebhookSecret", out var secretObj) || secretObj is not string webhookSecret)
        {
             return Error.Failure("Stripe.ConfigurationMissing", "Webhook Secret is not configured.");
        }

        try
        {
            // Security: Verify Signature
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            // Business Rule: Only handle Succeeded events for fulfillment
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                if (stripeEvent.Data.Object is PaymentIntent paymentIntent && 
                    paymentIntent.Metadata.TryGetValue("OrderReference", out var orderIdStr) &&
                    Guid.TryParse(orderIdStr, out var orderId) &&
                    paymentIntent.Metadata.TryGetValue("PaymentReference", out var paymentIdStr) &&
                    Guid.TryParse(paymentIdStr, out var paymentId))
                {
                    // Dispatch Domain Event to Core
                    await mediator.Publish(new PaymentEvents.ExternalPaymentCaptured(
                        OrderId: orderId,
                        PaymentId: paymentId,
                        TransactionId: paymentIntent.Id,
                        AmountCents: paymentIntent.Amount,
                        OccurredAt: DateTimeOffset.UtcNow
                    ));
                    
                    logger.LogInformation("Successfully processed Stripe webhook for Order {OrderId}", orderId);
                }
                else
                {
                    logger.LogWarning("Stripe webhook received but missing OrderReference/PaymentReference metadata.");
                }
            }

            return Result.Success;
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            return Error.Validation("Stripe.InvalidSignature", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing Stripe webhook.");
            return Error.Failure("Stripe.ProcessingError", ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> RefundAsync(
        DomainPaymentMethod settings,
        Payment payment,
        long amountCents,
        CancellationToken ct = default)
    {
        if (!settings.PrivateMetadata.TryGetValue("SecretKey", out var secretObj) || secretObj is not string secretKey)
             return Error.Failure("Stripe.ConfigurationMissing", "Stripe Secret Key is not configured.");

        try
        {
            var client = new StripeClient(secretKey);
            var service = new RefundService(client);

            var options = new RefundCreateOptions
            {
                PaymentIntent = payment.ReferenceTransactionId,
                Amount = amountCents,
                Metadata = new Dictionary<string, string>
                {
                    ["Reason"] = "Order Canceled"
                }
            };

            await service.CreateAsync(options, cancellationToken: ct);
            return Result.Success;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe Refund failed for Payment {Id}", payment.Id);
            return Error.Failure("Stripe.RefundError", ex.StripeError.Message ?? "Unknown Stripe error");
        }
    }
}
