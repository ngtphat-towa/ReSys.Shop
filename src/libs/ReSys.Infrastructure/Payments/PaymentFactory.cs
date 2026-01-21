using ErrorOr;
using ReSys.Core.Domain.Ordering.Payments.Gateways;
using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Infrastructure.Payments;

/// <summary>
/// Factory responsible for resolving the correct IPaymentProcessor strategy
/// based on the PaymentMethod configuration.
/// </summary>
public sealed class PaymentFactory(IEnumerable<IPaymentProcessor> processors) : IPaymentProcessorFactory
{
    /// <summary>
    /// Retrieves the processor implementation for the specified payment type.
    /// </summary>
    public ErrorOr<IPaymentProcessor> GetProcessor(PaymentMethod.PaymentType type)
    {
        // Guard: Ensure strategy exists
        var processor = processors.FirstOrDefault(p => p.Type == type);
        
        if (processor is null)
        {
            return Error.Failure(
                code: "Payment.ProcessorMissing", 
                description: $"No payment processor implementation found for type '{type}'.");
        }

        return ErrorOrFactory.From(processor);
    }
}
