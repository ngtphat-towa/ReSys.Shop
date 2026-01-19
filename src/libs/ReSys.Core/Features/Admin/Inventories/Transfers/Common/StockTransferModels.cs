using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Admin.Inventories.Transfers.Common;

public record StockTransferInput
{
    public Guid SourceLocationId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public string? Reason { get; set; }
}

public record StockTransferListItem
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public Guid SourceLocationId { get; set; }
    public string SourceLocationName { get; set; } = null!;
    public Guid DestinationLocationId { get; set; }
    public string DestinationLocationName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

public record StockTransferDetail : StockTransferListItem
{
    public string? Reason { get; set; }
    public List<StockTransferItemModel> Items { get; set; } = [];
}

public record StockTransferItemModel
{
    public Guid VariantId { get; set; }
    public string Sku { get; set; } = null!;
    public string VariantName { get; set; } = null!;
    public int Quantity { get; set; }
}

public record AddTransferItemRequest(Guid VariantId, int Quantity);
