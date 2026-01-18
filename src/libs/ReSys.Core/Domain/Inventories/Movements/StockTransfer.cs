using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Movements;

/// <summary>
/// Aggregate root responsible for orchestrating the atomic movement of stock between locations.
/// Manages the "In-Transit" state to ensure inventory is never doubled or lost.
/// </summary>
public sealed class StockTransfer : Aggregate, IAuditable
{
    public string ReferenceNumber { get; private set; } = string.Empty;
    public Guid SourceLocationId { get; private set; }
    public Guid DestinationLocationId { get; private set; }
    public StockTransferStatus Status { get; private set; } = StockTransferStatus.Draft;
    public string? Reason { get; private set; }

    public ICollection<StockTransferItem> Items { get; private set; } = new List<StockTransferItem>();

    private StockTransfer() { }

    /// <summary>
    /// Factory for creating a new stock movement process.
    /// </summary>
    public static ErrorOr<StockTransfer> Create(
        Guid sourceLocationId,
        Guid destinationLocationId,
        string? referenceNumber = null,
        string? reason = null)
    {
        // Guard: Prevent logical loops
        if (sourceLocationId == destinationLocationId)
            return StockTransferErrors.SameLocation;

        var transfer = new StockTransfer
        {
            Id = Guid.NewGuid(),
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            ReferenceNumber = referenceNumber ?? $"{StockTransferConstraints.ReferenceNumberPrefix}{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Reason = reason?.Trim(),
            Status = StockTransferStatus.Draft
        };

        transfer.RaiseDomainEvent(new StockTransferEvents.StockTransferCreated(transfer));
        return transfer;
    }

    /// <summary>
    /// Adds or updates an item in the transfer list.
    /// </summary>
    public ErrorOr<Success> AddItem(Guid variantId, int quantity)
    {
        // Guard: Prevent modification of in-flight transfers
        if (Status != StockTransferStatus.Draft)
            return StockTransferErrors.InvalidStatusTransition(Status, nameof(AddItem));

        // Guard: Prevent non-positive quantities
        if (quantity <= 0)
            return Error.Validation("StockTransfer.InvalidQuantity", "Quantity must be greater than zero.");

        var existing = Items.FirstOrDefault(x => x.VariantId == variantId);
        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            var newItem = new StockTransferItem(Id, variantId, quantity);
            Items.Add(newItem);
            RaiseDomainEvent(new StockTransferEvents.StockTransferItemAdded(this, newItem));
        }

        return Result.Success;
    }

    /// <summary>
    /// Marks the transfer as left the source location. 
    /// Physical stock should be deducted from source at this point.
    /// </summary>
    public ErrorOr<Success> Ship()
    {
        // Guard: Workflow enforcement
        if (Status != StockTransferStatus.Draft)
            return StockTransferErrors.InvalidStatusTransition(Status, nameof(Ship));

        // Guard: Prevent empty paperwork
        if (!Items.Any())
            return StockTransferErrors.EmptyTransfer;

        Status = StockTransferStatus.InTransit;
        RaiseDomainEvent(new StockTransferEvents.StockTransferShipped(this));
        return Result.Success;
    }

    /// <summary>
    /// Marks the transfer as arrived and accepted at the destination.
    /// Physical stock should be added to destination at this point.
    /// </summary>
    public ErrorOr<Success> Receive()
    {
        // Guard: Only items 'on the truck' can be received
        if (Status != StockTransferStatus.InTransit)
            return StockTransferErrors.InvalidStatusTransition(Status, nameof(Receive));

        Status = StockTransferStatus.Completed;
        RaiseDomainEvent(new StockTransferEvents.StockTransferReceived(this));
        return Result.Success;
    }

    /// <summary>
    /// Aborts the transfer process.
    /// </summary>
    public ErrorOr<Success> Cancel()
    {
        // Guard: Cannot cancel a completed financial/physical movement
        if (Status == StockTransferStatus.Completed)
            return StockTransferErrors.InvalidStatusTransition(Status, nameof(Cancel));

        Status = StockTransferStatus.Canceled;
        RaiseDomainEvent(new StockTransferEvents.StockTransferCanceled(this));
        return Result.Success;
    }
}

/// <summary>
/// Represents a specific SKU and quantity within a transfer.
/// </summary>
public sealed class StockTransferItem : Entity
{
    public Guid StockTransferId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }

    internal StockTransferItem(Guid stockTransferId, Guid variantId, int quantity)
    {
        Id = Guid.NewGuid();
        StockTransferId = stockTransferId;
        VariantId = variantId;
        Quantity = quantity;
    }

    internal void UpdateQuantity(int newQuantity) => Quantity = newQuantity;
}
