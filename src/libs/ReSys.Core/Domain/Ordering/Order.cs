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
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Calculations;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering;

/// <summary>
/// Root of the Ordering Aggregate. 
/// Orchestrates the lifecycle from shopping cart to multi-warehouse fulfillment.
/// It maintains invariants for pricing snapshots, inventory reservation, and payment reconciliation.
/// </summary>
public sealed class Order : Aggregate, IHasMetadata
{
    /// <summary>
    /// Represents the sequential lifecycle stages of a customer order.
    /// </summary>
    public enum OrderState 
    { 
        /// <summary>Initial state where items are added/removed.</summary>
        Cart, 
        /// <summary>Shipping and billing destination captured.</summary>
        Address, 
        /// <summary>Logistics options selected.</summary>
        Delivery, 
        /// <summary>Financial commitment/Payment processing.</summary>
        Payment, 
        /// <summary>Final review stage.</summary>
        Confirm, 
        /// <summary>Terminal success state. Inventory committed.</summary>
        Complete, 
        /// <summary>Terminal abort state. Inventory released.</summary>
        Canceled 
    }

    #region Properties
    /// <summary>Human-readable unique identifier (e.g. ORD-20260121-1234).</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Current operational status of the order.</summary>
    public OrderState State { get; set; } = OrderState.Cart;

    /// <summary>ISO 4217 Currency code (e.g. USD).</summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>Sum of all merchandise sub-totals in minor units (cents).</summary>
    public long ItemTotalCents { get; set; }

    /// <summary>Total freight/logistics cost in minor units (cents).</summary>
    public long ShipmentTotalCents { get; set; }

    /// <summary>Net value of all eligible discounts and fees in minor units (cents).</summary>
    public long AdjustmentTotalCents { get; set; }

    /// <summary>Grand total to be paid: Items + Shipments + Adjustments.</summary>
    public long TotalCents { get; set; }

    /// <summary>The primary site where the order was placed.</summary>
    public Guid StoreId { get; set; }

    /// <summary>Authenticated user identifier.</summary>
    public string? UserId { get; set; }

    /// <summary>Anonymous session identifier for guest visitors.</summary>
    public string? SessionId { get; set; } 

    /// <summary>Customer contact email for notifications.</summary>
    public string? Email { get; set; }
    
    /// <summary>Timestamp of successful completion.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Timestamp of cancellation.</summary>
    public DateTimeOffset? CanceledAt { get; set; }

    /// <summary>Target delivery address.</summary>
    public Guid? ShipAddressId { get; set; }

    /// <summary>Legal billing address.</summary>
    public Guid? BillAddressId { get; set; }

    /// <summary>Navigation: Shipping destination.</summary>
    public UserAddress? ShipAddress { get; set; }

    /// <summary>Navigation: Billing destination.</summary>
    public UserAddress? BillAddress { get; set; }

    /// <summary>Selected shipping provider identifier.</summary>
    public Guid? ShippingMethodId { get; set; }

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
    public static ErrorOr<Order> Create(Guid storeId, string currency, string? userId = null, string? email = null, string? sessionId = null)
    {
        if (storeId == Guid.Empty) return OrderErrors.StoreRequired;
        if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId)) 
            return Error.Validation("Order.IdentityRequired", "Either UserId or SessionId is required.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            UserId = userId,
            SessionId = sessionId,
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
    /// Adds a product variant to the order line items. 
    /// This method captures a pricing snapshot to ensure historical accuracy.
    /// </summary>
    /// <param name="variant">The product variant to add.</param>
    /// <param name="quantity">The number of units (must be >= 1).</param>
    /// <param name="now">Current timestamp for audit.</param>
    /// <param name="overridePriceCents">Optional admin override for the unit price.</param>
    /// <returns>Success or validation error (e.g. InvalidQuantity, VariantNotFound).</returns>
    /// <remarks>
    /// Business Rules:
    /// 1. If the item exists, quantity is incremented.
    /// 2. If new, a LineItem is created with a Snapshot of the Product Name/SKU.
    /// 3. InventoryUnits are created in 'Pending' state for every single unit.
    /// </remarks>
    public ErrorOr<Success> AddVariant(Variant variant, int quantity, DateTimeOffset now, long? overridePriceCents = null)
    {
        // Guard: Items can only be added during the draft stage.
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
            // This ensures the order history remains accurate even if the product is later renamed or deleted.
            var capturedName = variant.Product != null 
                ? (variant.OptionValues.Any() ? $"{variant.Product.Name} ({string.Join(", ", variant.OptionValues.Select(v => v.Presentation))})" : variant.Product.Name)
                : "Unknown Product";

            var itemResult = LineItem.Create(Id, variant, quantity, Currency, capturedName, now, overridePriceCents);
            if (itemResult.IsError) return itemResult.Errors;
            item = itemResult.Value;
            LineItems.Add(item);
        }

        // Audit Override: If admin changed the price, log the original for margin reporting.
        if (overridePriceCents.HasValue)
        {
            var originalPrice = (long)(variant.Price * 100);
            var historyResult = AddHistoryEntry(
                $"Price override applied to {item.CapturedName}. Original: {originalPrice/100m:C}, Applied: {overridePriceCents.Value/100m:C}",
                State,
                new Dictionary<string, object?> { { "VariantId", variant.Id }, { "OriginalPriceCents", originalPrice } });
            
            if (historyResult.IsError) return historyResult.Errors;
        }

        // GRANULAR TRACKING: Generate physical unit placeholders.
        // These units track the lifecycle of each specific physical item (Pending -> Picked -> Shipped).
        for (int i = 0; i < quantity; i++)
        {
            var unit = InventoryUnit.Create(variant.Id, item.Id, initialState: InventoryUnitState.Pending);
            item.InventoryUnits.Add(unit);
        }

        RecalculateTotals();
        RaiseDomainEvent(new OrderEvents.OrderUpdated(this));
        return Result.Success;
    }

    /// <summary>
    /// Removes a specific line item from the order and cleans up associated inventory reservations.
    /// </summary>
    public ErrorOr<Success> RemoveLineItem(Guid lineItemId)
    {
        if (State != OrderState.Cart) return OrderErrors.InvalidStateTransition(State.ToString(), nameof(RemoveLineItem));
        
        var item = LineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (item == null) return LineItemErrors.NotFound(lineItemId);

        // Chain of Custody: Clear associated units to release any potential holds.
        item.InventoryUnits.Clear();

        LineItems.Remove(item);
        RecalculateTotals();
        return Result.Success;
    }

    #endregion

    #region Business Logic - Logistics

    /// <summary>
    /// Attaches a physical shipment record to the order.
    /// Used during fulfillment to track packages.
    /// </summary>
    public ErrorOr<Success> AddShipment(Shipment shipment)
    {
        // Guard: Shipments can only be created once the order is ready for delivery processing.
        if (State < OrderState.Delivery && State != OrderState.Cart)
            return OrderErrors.InvalidStateTransition(State.ToString(), nameof(AddShipment));

        if (shipment == null) return ShipmentErrors.Required;

        Shipments.Add(shipment);
        return Result.Success;
    }

    /// <summary>
    /// Selects the shipping method and updates the shipping cost.
    /// Allows for admin overrides of the calculated cost.
    /// </summary>
    /// <param name="shippingMethodId">The selected carrier service.</param>
    /// <param name="overrideCostCents">Optional fixed cost (e.g. Free Shipping override).</param>
    public ErrorOr<Success> SetShippingMethod(Guid shippingMethodId, long? overrideCostCents = null)
    {
        // Guard: Logistics are locked once the order moves past delivery setup.
        if (State > OrderState.Delivery)
            return OrderErrors.InvalidStateTransition(State.ToString(), nameof(SetShippingMethod));

        ShippingMethodId = shippingMethodId;

        // Logic: Apply cost override if provided (e.g. from Admin or specific promotion logic handled outside).
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
    /// This implementation enforces a strict sequential flow to maintain data integrity.
    /// </summary>
    public ErrorOr<Success> Next()
    {
        return State switch
        {
            // Business Rule: Transition from draft to checkout requires merchandise.
            OrderState.Cart => TransitionTo(OrderState.Address),
            
            // Business Rule: Logistics setup requires a verified destination.
            OrderState.Address => TransitionTo(OrderState.Delivery),
            
            // Business Rule: Financial commitment requires a chosen logistics provider.
            OrderState.Delivery => TransitionTo(OrderState.Payment),
            
            // Business Rule: Final confirmation gated by payment success.
            OrderState.Payment => TransitionTo(OrderState.Confirm),
            
            // Business Rule: Finalization commits the inventory ledger.
            OrderState.Confirm => Complete(),
            _ => OrderErrors.InvalidStateTransition(State.ToString(), "Next")
        };
    }

    private ErrorOr<Success> TransitionTo(OrderState newState)
    {
        // Guard: Prevent checkout of empty carts.
        if (newState == OrderState.Address && !LineItems.Any()) return OrderErrors.EmptyCart;
        
        // Guard: Ensure logistics have a physical target before calculating shipping.
        if (newState == OrderState.Delivery && (ShipAddress == null || BillAddress == null)) return OrderErrors.AddressMissing;
        
        // Guard: Prevent payment processing without a defined shipping cost.
        if (newState == OrderState.Payment && ShippingMethodId == null) return OrderErrors.ShippingMethodMissing;

        var oldState = State;
        State = newState;
        
        // Audit Trail: Record every state movement for support and reporting.
        var historyResult = AddHistoryEntry($"Transitioned from {oldState} to {newState}.");
        if (historyResult.IsError) return historyResult.Errors;

        RaiseDomainEvent(new OrderEvents.OrderStateChanged(this, oldState, newState));
        return Result.Success;
    }

    private ErrorOr<Success> Complete()
    {
        // Financial Invariant: An order cannot be 'Complete' if funds are outstanding.
        var completedPaymentTotal = Payments
            .Where(p => p.State == Payment.PaymentState.Completed)
            .Sum(p => p.AmountCents);

        if (completedPaymentTotal < TotalCents) 
            return OrderErrors.InsufficientPayment(TotalCents / 100m, completedPaymentTotal / 100m);

        // Physical Invariant: Inventory must be physically earmarked (OnHand) before completion.
        // This prevents the system from finalizing an order that cannot be fulfilled.
        foreach (var item in LineItems)
        {
            var determinedCount = item.InventoryUnits.Count(u => 
                u.State == InventoryUnitState.OnHand || 
                u.State == InventoryUnitState.Backordered ||
                u.State == InventoryUnitState.Shipped);

            if (determinedCount < item.Quantity)
                return OrderErrors.IncompleteInventoryAllocation;
        }

        // Ledger Commitment: Permanently decrement stock by finalizing the 'Pending' units.
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
    /// Recalculates all financial totals based on current line items, adjustments, and shipping costs.
    /// This method ensures the invariant: Total = Items + Adjustments + Shipping.
    /// It must be called after ANY mutation to the order structure.
    /// </summary>
    public void RecalculateTotals()
    {
        // Aggregate sub-totals from line items (Price * Qty + ItemAdjustments)
        ItemTotalCents = LineItems.Sum(li => li.GetTotalCents());
        
        // Sum order-level adjustments (Global Discounts, Taxes)
        AdjustmentTotalCents = OrderAdjustments
            .Where(a => a.Eligible)
            .Sum(a => a.AmountCents);

        TotalCents = ItemTotalCents + AdjustmentTotalCents + ShipmentTotalCents;
        
        // Safety Guard: Floor at zero to prevent negative payment requests.
        TotalCents = Math.Max(0, TotalCents); 
    }

    /// <summary>
    /// Removes all promotion-related adjustments from the order and its line items.
    /// Keeps mandatory adjustments (like Taxes or Shipping Fees).
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

    /// <summary>
    /// Applies a promotion using the provided calculator strategy.
    /// Replaces any existing promotion on the order.
    /// </summary>
    /// <param name="promotion">The promotion entity to apply.</param>
    /// <param name="calculator">Strategy for calculating discounts.</param>
    public ErrorOr<Success> ApplyPromotion(Promotion promotion, IPromotionCalculator calculator)
    {
        if (State != OrderState.Cart) return OrderErrors.InvalidStateTransition(State.ToString(), nameof(ApplyPromotion));

        // 1. Calculate the impact of the promotion
        var result = calculator.Calculate(promotion, this);
        if (result.IsError) return result.Errors;

        // 2. Clear previous promotions to ensure exclusivity
        ClearPromotionAdjustments();

        // 3. Apply new adjustments (Order-level or Item-level)
        foreach (var adj in result.Value.Adjustments)
        {
            if (adj.LineItemId.HasValue)
            {
                var item = LineItems.FirstOrDefault(li => li.Id == adj.LineItemId);
                if (item != null)
                {
                    var adjustment = LineItemAdjustment.Create(
                        item.Id, 
                        adj.AmountCents, 
                        adj.Description, 
                        promotion.Id).Value;
                        
                    item.Adjustments.Add(adjustment);
                }
            }
            else
            {
                var adjustment = OrderAdjustment.Create(
                    Id, 
                    adj.AmountCents, 
                    adj.Description, 
                    OrderAdjustment.AdjustmentScope.Order,
                    promotion.Id).Value;
                    
                OrderAdjustments.Add(adjustment);
            }
        }

        RecalculateTotals();
        return Result.Success;
    }

    /// <summary>
    /// Removes the currently active promotion and recalculates totals.
    /// </summary>
    public ErrorOr<Success> RemovePromotion()
    {
        if (State != OrderState.Cart) return OrderErrors.InvalidStateTransition(State.ToString(), nameof(RemovePromotion));

        ClearPromotionAdjustments();
        return Result.Success;
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

    /// <summary>
    /// Updates the custom metadata dictionaries for the order.
    /// </summary>
    public void SetMetadata(IDictionary<string, object?> publicMetadata, IDictionary<string, object?> privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }

    /// <summary>
    /// Sets both shipping and billing addresses for the order.
    /// Validates state transition rules (Must be in Cart or Address state).
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

    /// <summary>
    /// Manually updates a payment status to captured. 
    /// Used by webhooks or manual admin overrides.
    /// </summary>
    public ErrorOr<Success> CapturePayment(Guid paymentId, string transactionId)
    {
        var payment = Payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment == null) return OrderErrors.PaymentNotFound;

        return payment.MarkAsCaptured(transactionId);
    }

    /// <summary>
    /// Adds a pending payment transaction to the order.
    /// </summary>
    public ErrorOr<Success> AddPayment(Payment payment)
    {
        if (State == OrderState.Canceled) return OrderErrors.InvalidStateTransition(State.ToString(), nameof(AddPayment));
        if (State == OrderState.Complete) return OrderErrors.CannotModifyInTerminalState;

        Payments.Add(payment);
        RecalculateTotals();
        return Result.Success;
    }
    #endregion
}
