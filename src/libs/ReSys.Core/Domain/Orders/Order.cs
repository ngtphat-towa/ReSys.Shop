using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Orders.LineItems;
using ReSys.Core.Domain.Orders.Payments;
using ReSys.Core.Domain.Orders.Shipments;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ErrorOr;

namespace ReSys.Core.Domain.Orders;

public sealed class Order : Aggregate, IHasMetadata
{
    public string Number { get; private set; } = string.Empty;
    public OrderState State { get; private set; } = OrderState.Cart;
    public string Currency { get; private set; } = "USD";
    
    // Totals
    public decimal ItemTotal { get; private set; }
    public decimal ShipmentTotal { get; private set; }
    public decimal AdjustmentTotal { get; private set; }
    public decimal Total { get; private set; }

    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; private set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; private set; } = new Dictionary<string, object?>();

    // Encapsulated Collections
    private readonly List<LineItem> _lineItems = [];
    public IReadOnlyCollection<LineItem> LineItems => _lineItems.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private readonly List<Shipment> _shipments = [];
    public IReadOnlyCollection<Shipment> Shipments => _shipments.AsReadOnly();

    private Order() { }

    public static Order Create(string? userId, string? email, string currency = "USD")
    {
        var order = new Order
        {
            Number = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            UserId = userId,
            Email = email,
            Currency = currency,
            State = OrderState.Cart
        };

        order.RaiseDomainEvent(new OrderEvents.OrderCreated(order));
        return order;
    }

    public ErrorOr<LineItem> AddVariant(Variant variant, int quantity)
    {
        if (State != OrderState.Cart)
            return OrderErrors.InvalidStateTransition(State.ToString(), "Adding Items");

        if (quantity <= 0)
            return LineItemErrors.InvalidQuantity;

        var existingItem = _lineItems.FirstOrDefault(li => li.VariantId == variant.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = LineItem.Create(Id, variant, quantity);
            _lineItems.Add(newItem);
        }

        Recalculate();
        RaiseDomainEvent(new OrderEvents.ItemAddedToOrder(this, _lineItems.Last()));
        return _lineItems.Last();
    }

    public ErrorOr<Success> RemoveItem(Guid lineItemId)
    {
        if (State != OrderState.Cart)
            return OrderErrors.InvalidStateTransition(State.ToString(), "Removing Items");

        var item = _lineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (item == null) return LineItemErrors.NotFound(lineItemId);

        _lineItems.Remove(item);
        Recalculate();
        RaiseDomainEvent(new OrderEvents.ItemRemovedFromOrder(this, lineItemId));
        return Result.Success;
    }

    public void Recalculate()
    {
        ItemTotal = _lineItems.Sum(li => li.Total);
        Total = ItemTotal + ShipmentTotal + AdjustmentTotal;
    }

    public ErrorOr<Success> TransitionTo(OrderState newState)
    {
        // Simple state machine logic
        if (!IsValidTransition(State, newState))
            return OrderErrors.InvalidStateTransition(State.ToString(), newState.ToString());

        var oldState = State;
        State = newState;

        if (newState == OrderState.Complete)
            CompletedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OrderEvents.OrderStateChanged(this, oldState, newState));
        return Result.Success;
    }

    private static bool IsValidTransition(OrderState current, OrderState next)
    {
        return (current, next) switch
        {
            (OrderState.Cart, OrderState.Address) => true,
            (OrderState.Address, OrderState.Delivery) => true,
            (OrderState.Delivery, OrderState.Payment) => true,
            (OrderState.Payment, OrderState.Confirm) => true,
            (OrderState.Confirm, OrderState.Complete) => true,
            (_, OrderState.Canceled) => current != OrderState.Complete,
            _ => false
        };
    }

    public enum OrderState { Cart, Address, Delivery, Payment, Confirm, Complete, Canceled }
}
