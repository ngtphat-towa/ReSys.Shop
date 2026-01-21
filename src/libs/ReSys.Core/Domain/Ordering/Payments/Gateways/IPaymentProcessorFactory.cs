using ErrorOr;
using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.Domain.Ordering.Payments.Gateways;

public interface IPaymentProcessorFactory
{
    ErrorOr<IPaymentProcessor> GetProcessor(PaymentMethod.PaymentType type);
}
