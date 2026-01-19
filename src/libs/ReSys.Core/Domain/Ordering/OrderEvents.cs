using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Ordering.LineItems;

namespace ReSys.Core.Domain.Ordering;

public static class OrderEvents
{
    public record OrderCreated(Order Order) : IDomainEvent;
    public record OrderUpdated(Order Order) : IDomainEvent;
    public record OrderItemAdded(Order Order, LineItem Item) : IDomainEvent;
    public record OrderItemRemoved(Order Order, Guid LineItemId) : IDomainEvent;
    public record OrderStateChanged(Order Order, Order.OrderState OldState, Order.OrderState NewState) : IDomainEvent;
    public record OrderCompleted(Order Order) : IDomainEvent;
    public record OrderCanceled(Order Order, string? Reason) : IDomainEvent;
}
