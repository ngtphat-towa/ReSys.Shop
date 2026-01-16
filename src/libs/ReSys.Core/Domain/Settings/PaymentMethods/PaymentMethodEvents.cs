using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Settings.PaymentMethods;

public static class PaymentMethodEvents
{
    public record PaymentMethodCreated(PaymentMethod PaymentMethod) : IDomainEvent;
    public record PaymentMethodUpdated(PaymentMethod PaymentMethod) : IDomainEvent;
    public record PaymentMethodDeleted(PaymentMethod PaymentMethod) : IDomainEvent;
}
