using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Orders.LineItems;

namespace ReSys.Core.Domain.Orders;

public static class OrderEvents
{
    public record OrderCreated(Order Order) : IDomainEvent;
    public record ItemAddedToOrder(Order Order, LineItem Item) : IDomainEvent;
    public record ItemRemovedFromOrder(Order Order, Guid LineItemId) : IDomainEvent;
    public record OrderStateChanged(Order Order, Order.OrderState OldState, Order.OrderState NewState) : IDomainEvent;
    public record OrderCompleted(Order Order) : IDomainEvent;
    public record OrderCanceled(Order Order, string Reason) : IDomainEvent;
}