using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Infrastructure.Payments;
using ReSys.Core.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace ReSys.Api.Controllers;

[Route("api/webhooks")]
[ApiController]
public class WebhookController(
    PaymentFactory paymentFactory,
    IApplicationDbContext dbContext,
    ILogger<WebhookController> logger) : ControllerBase
{
    /// <summary>
    /// Handles incoming webhooks from payment providers.
    /// </summary>
    /// <param name="provider">The provider name (e.g., Stripe, PayPal).</param>
    [HttpPost("{provider}")]
    public async Task<IActionResult> HandleWebhook(string provider)
    {
        // 1. Validate Provider
        if (!Enum.TryParse<PaymentMethod.PaymentType>(provider, true, out var type))
        {
            return BadRequest($"Unknown provider: {provider}");
        }

        // 2. Load Configuration (Settings)
        // We need the PaymentMethod entity to get the WebhookSecret
        var paymentMethod = await dbContext.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Type == type && pm.Status == PaymentMethod.PaymentStatus.Active);

        if (paymentMethod == null)
        {
            logger.LogWarning("Received webhook for {Provider} but no active PaymentMethod found.", provider);
            return BadRequest("Provider not configured or inactive.");
        }

        // 3. Resolve Processor
        var processorResult = paymentFactory.GetProcessor(type);
        if (processorResult.IsError)
        {
             return BadRequest("Processor implementation not found.");
        }

        // 4. Read Body & Signature
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            json = await reader.ReadToEndAsync();
        }
        
        // Business Rule: Select the correct signature header for the provider
        var signatureHeader = type switch
        {
            PaymentMethod.PaymentType.Stripe => "Stripe-Signature",
            _ => "X-Hub-Signature" // Common default (e.g. GitHub, Facebook)
        };

        var signature = HttpContext.Request.Headers[signatureHeader];
        if (string.IsNullOrEmpty(signature))
        {
            logger.LogWarning("Missing signature header {Header} for provider {Provider}", signatureHeader, provider);
            return BadRequest("Missing signature.");
        }

        // 5. Process
        var result = await processorResult.Value.ProcessWebhookAsync(paymentMethod, json, signature!);

        if (result.IsError)
        {
            logger.LogWarning("Webhook failed: {Error}", result.FirstError.Description);
            return BadRequest(result.FirstError.Description);
        }

        return Ok();
    }
}
