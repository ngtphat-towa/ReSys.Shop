using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Settings.ShippingMethods;

public static class ShippingMethodEvents
{
    public record ShippingMethodCreated(ShippingMethod ShippingMethod) : IDomainEvent;
    public record ShippingMethodUpdated(ShippingMethod ShippingMethod) : IDomainEvent;
    public record ShippingMethodDeleted(ShippingMethod ShippingMethod) : IDomainEvent;
    public record ShippingMethodRestored(ShippingMethod ShippingMethod) : IDomainEvent;
    public record ShippingMethodActivated(ShippingMethod ShippingMethod) : IDomainEvent;
    public record ShippingMethodDeactivated(ShippingMethod ShippingMethod) : IDomainEvent;
}