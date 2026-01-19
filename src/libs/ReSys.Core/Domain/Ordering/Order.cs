using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Ordering.Adjustments;
using ReSys.Core.Domain.Ordering.History;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Domain.Settings.ShippingMethods;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering;

/// <summary>
/// Root of the Ordering Aggregate. 
/// Orchestrates the lifecycle from shopping cart to multi-warehouse fulfillment.
/// </summary>
public sealed class Order : Aggregate, IHasMetadata
{
    public enum OrderState { Cart, Address, Delivery, Payment, Confirm, Complete, Canceled }

    #region Properties
    public string Number { get; set; } = string.Empty;
    public OrderState State { get; set; } = OrderState.Cart;
    public string Currency { get; set; } = "USD";
    
    // Financial Ledger (Minor Units - long)
    public long ItemTotalCents { get; set; }
    public long ShipmentTotalCents { get; set; }
    public long AdjustmentTotalCents { get; set; }
    public long TotalCents { get; set; }

    // Context
    public Guid StoreId { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    
    // Lifecycle Tracking
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }

    // Delivery Context
    public Guid? ShipAddressId { get; set; }
    public Guid? BillAddressId { get; set; }
    public UserAddress? ShipAddress { get; set; }
    public UserAddress? BillAddress { get; set; }
    public Guid? ShippingMethodId { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
    #endregion

    #region Collections
    public ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; } = new List<OrderAdjustment>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<OrderHistory> Histories { get; set; } = new List<OrderHistory>();
    #endregion

    public Order() { }

    /// <summary>
    /// Factory for creating a new sales document.
    /// </summary>
    public static ErrorOr<Order> Create(Guid storeId, string currency, string? userId = null, string? email = null)
    {
        if (storeId == Guid.Empty) return OrderErrors.StoreRequired;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            UserId = userId,
            Email = email?.Trim().ToLowerInvariant(),
            Currency = currency.Trim().ToUpperInvariant(),
            Number = $"{OrderConstraints.NumberPrefix}{DateTimeOffset.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}",
            State = OrderState.Cart
        };

        var historyResult = order.AddHistoryEntry("Cart initialized.");
        if (historyResult.IsError) return historyResult.Errors;

        order.RaiseDomainEvent(new OrderEvents.OrderCreated(order));
        return order;
    }

    #region Business Logic - Items

    /// <summary>
    /// Adds a product variant to the order. Captures pricing and naming truth immediately.
    /// Supports optional admin price overrides.
    /// </summary>
    public ErrorOr<Success> AddVariant(Variant variant, int quantity, DateTimeOffset now, long? overridePriceCents = null)
    {
        if (State != OrderState.Cart) return OrderErrors.InvalidStateTransition(State.ToString(), nameof(AddVariant));
        if (quantity < 1) return LineItemErrors.InvalidQuantity;

        var existing = LineItems.FirstOrDefault(li => li.VariantId == variant.Id);
        LineItem item;

        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity, now);
            item = existing;
        }
        else
        {
            // SNAPSHOT: Construct descriptive name from variant/product metadata
            var capturedName = variant.Product != null 
                ? (variant.OptionValues.Any() ? $"{variant.Product.Name} ({string.Join(", ", variant.OptionValues.Select(v => v.Presentation))})" : variant.Product.Name)
                : "Unknown Product";

            var itemResult = LineItem.Create(Id, variant, quantity, Currency, capturedName, now, overridePriceCents);
            if (itemResult.IsError) return itemResult.Errors;
            item = itemResult.Value;
            LineItems.Add(item);
        }

        // Audit Override: If admin changed the price, log the original for margin reporting
        if (overridePriceCents.HasValue)
        {
            var originalPrice = (long)(variant.Price * 100);
            var historyResult = AddHistoryEntry(
                $"Price override applied to {item.CapturedName}. Original: {originalPrice/100m:C}, Applied: {overridePriceCents.Value/100m:C}",
                State,
                new Dictionary<string, object?> { { "VariantId", variant.Id }, { "OriginalPriceCents", originalPrice } });
            
            if (historyResult.IsError) return historyResult.Errors;
        }

        // GRANULAR TRACKING: Generate physical unit placeholders
        for (int i = 0; i < quantity; i++)
        {
            var unit = InventoryUnit.Create(variant.Id, item.Id, initialState: InventoryUnitState.Pending);
            item.InventoryUnits.Add(unit);
        }

        RecalculateTotals();
        RaiseDomainEvent(new OrderEvents.OrderUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> RemoveLineItem(Guid lineItemId)
    {
        if (State != OrderState.Cart) return OrderErrors.InvalidStateTransition(State.ToString(), nameof(RemoveLineItem));
        
        var item = LineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (item == null) return LineItemErrors.NotFound(lineItemId);

        // Chain of Custody: Clear associated units
        item.InventoryUnits.Clear();

        LineItems.Remove(item);
        RecalculateTotals();
        return Result.Success;
    }

    #endregion

    #region Business Logic - Logistics

    /// <summary>
    /// Links a new physical shipment to the order.
    /// </summary>
    public ErrorOr<Success> AddShipment(Shipment shipment)
    {
        if (State < OrderState.Delivery && State != OrderState.Cart)
            return OrderErrors.InvalidStateTransition(State.ToString(), nameof(AddShipment));

        if (shipment == null) return ShipmentErrors.Required;

        Shipments.Add(shipment);
        return Result.Success;
    }

    /// <summary>
    /// Assigns a shipping method and calculates or overrides the shipping cost.
    /// </summary>
    public ErrorOr<Success> SetShippingMethod(Guid shippingMethodId, long? overrideCostCents = null)
    {
        if (State > OrderState.Delivery)
            return OrderErrors.InvalidStateTransition(State.ToString(), nameof(SetShippingMethod));

        ShippingMethodId = shippingMethodId;

        if (overrideCostCents.HasValue)
        {
            ShipmentTotalCents = overrideCostCents.Value;
            var historyResult = AddHistoryEntry($"Shipping cost overridden by admin to {ShipmentTotalCents/100m:C}.", State);
            if (historyResult.IsError) return historyResult.Errors;
        }

        RecalculateTotals();
        return Result.Success;
    }

    #endregion

    #region Business Logic - State Machine

    /// <summary>
    /// Moves the order forward in its lifecycle after validating state-specific rules.
    /// </summary>
    public ErrorOr<Success> Next()
    {
        return State switch
        {
            OrderState.Cart => TransitionTo(OrderState.Address),
            OrderState.Address => TransitionTo(OrderState.Delivery),
            OrderState.Delivery => TransitionTo(OrderState.Payment),
            OrderState.Payment => TransitionTo(OrderState.Confirm),
            OrderState.Confirm => Complete(),
            _ => OrderErrors.InvalidStateTransition(State.ToString(), "Next")
        };
    }

    private ErrorOr<Success> TransitionTo(OrderState newState)
    {
        // Guard: Logic Invariants per state
        if (newState == OrderState.Address && !LineItems.Any()) return OrderErrors.EmptyCart;
        if (newState == OrderState.Delivery && (ShipAddress == null || BillAddress == null)) return OrderErrors.AddressMissing;
        if (newState == OrderState.Payment && ShippingMethodId == null) return OrderErrors.ShippingMethodMissing;

        var oldState = State;
        State = newState;
        
        var historyResult = AddHistoryEntry($"Transitioned from {oldState} to {newState}.");
        if (historyResult.IsError) return historyResult.Errors;

        RaiseDomainEvent(new OrderEvents.OrderStateChanged(this, oldState, newState));
        return Result.Success;
    }

    private ErrorOr<Success> Complete()
    {
        // Financial Guard: Payments must cover the total
        var completedPaymentTotal = Payments
            .Where(p => p.State == Payment.PaymentState.Completed)
            .Sum(p => p.AmountCents);

        if (completedPaymentTotal < TotalCents) 
            return OrderErrors.InsufficientPayment(TotalCents / 100m, completedPaymentTotal / 100m);

        // Physical Guard: Every unit must have a determined state (either ready to pick or recorded as backordered)
        foreach (var item in LineItems)
        {
            var determinedCount = item.InventoryUnits.Count(u => 
                u.State == InventoryUnitState.OnHand || 
                u.State == InventoryUnitState.Backordered ||
                u.State == InventoryUnitState.Shipped);

            if (determinedCount < item.Quantity)
                return OrderErrors.IncompleteInventoryAllocation;
        }

        // Finalize: Commit the physical ledger for all units. 
        // This transitions units from "Draft/Pending" to "Committed Transaction".
        foreach (var item in LineItems)
        {
            foreach (var unit in item.InventoryUnits)
            {
                unit.FinalizeUnit();
            }
        }

        State = OrderState.Complete;
        CompletedAt = DateTimeOffset.UtcNow;
        
        var historyResult = AddHistoryEntry("Order finalized and committed to fulfillment.");
        if (historyResult.IsError) return historyResult.Errors;

        RaiseDomainEvent(new OrderEvents.OrderCompleted(this));
        return Result.Success;
    }

    public ErrorOr<Success> Cancel(string? reason = null)
    {
        if (State == OrderState.Complete) return OrderErrors.CannotCancelCompleted;
        if (State == OrderState.Canceled) return Result.Success;
        
        State = OrderState.Canceled;
        CanceledAt = DateTimeOffset.UtcNow;

        // Chain of Custody: Release all inventory units
        foreach (var item in LineItems)
        {
            foreach (var unit in item.InventoryUnits)
            {
                unit.Cancel();
            }
        }
        
        var historyResult = AddHistoryEntry($"Order canceled. Reason: {reason ?? "Not specified"}");
        if (historyResult.IsError) return historyResult.Errors;

        RaiseDomainEvent(new OrderEvents.OrderCanceled(this, reason));
        return Result.Success;
    }

    #endregion

    #region Business Logic - Financials

    /// <summary>
    /// Recalculates all order totals. 
    /// Formula: (Items + LineAdjustments) + OrderAdjustments + Shipping = Grand Total.
    /// </summary>
    public void RecalculateTotals()
    {
        // Aggregate sub-totals from snapshots
        ItemTotalCents = LineItems.Sum(li => li.GetTotalCents());
        
        AdjustmentTotalCents = OrderAdjustments
            .Where(a => a.Eligible)
            .Sum(a => a.AmountCents);

        TotalCents = ItemTotalCents + AdjustmentTotalCents + ShipmentTotalCents;
        
        TotalCents = Math.Max(0, TotalCents); // Floor at zero
    }

    /// <summary>
    /// Removes promotional discounts while keeping manual tax/shipping fees.
    /// </summary>
    public void ClearPromotionAdjustments()
    {
        var orderPromos = OrderAdjustments.Where(a => a.PromotionId.HasValue).ToList();
        foreach (var promo in orderPromos) OrderAdjustments.Remove(promo);

        foreach (var item in LineItems)
        {
            var itemPromos = item.Adjustments.Where(a => a.PromotionId.HasValue).ToList();
            foreach (var promo in itemPromos) item.Adjustments.Remove(promo);
        }

        RecalculateTotals();
    }

    #endregion

    #region Helpers

    private ErrorOr<Success> AddHistoryEntry(string description)
    {
        return OrderHistory.Create(Id, description, State)
            .Then(entry => 
            {
                Histories.Add(entry);
                return Result.Success;
            });
    }

    private ErrorOr<Success> AddHistoryEntry(string description, OrderState state)
    {
        return OrderHistory.Create(Id, description, state)
            .Then(entry =>
            {
                Histories.Add(entry);
                return Result.Success;
            });
    }

    private ErrorOr<Success> AddHistoryEntry(string description, OrderState state, IDictionary<string, object?> metadata)
    {
        return OrderHistory.Create(
            orderId: Id, 
            description: description, 
            toState: state, 
            context: metadata)
            .Then(entry =>
            {
                Histories.Add(entry);
                return Result.Success;
            });
    }

    public void SetMetadata(IDictionary<string, object?> publicMetadata, IDictionary<string, object?> privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }

    /// <summary>
    /// Updates the shipping destination. Gated by state machine.
    /// </summary>
    public ErrorOr<Success> SetAddresses(UserAddress shipping, UserAddress billing)
    {
        if (State > OrderState.Address)
            return OrderErrors.InvalidStateTransition(State.ToString(), nameof(SetAddresses));

        ShipAddress = shipping;
        BillAddress = billing;
        ShipAddressId = shipping.Id;
        BillAddressId = billing.Id;

        return Result.Success;
    }

    #endregion
}
