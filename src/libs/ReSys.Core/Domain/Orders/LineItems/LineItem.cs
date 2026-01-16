using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Domain.Orders.LineItems;

public sealed class LineItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid VariantId { get; private set; }
    
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Total => Price * Quantity;
    
    // Snapshots
    public string CapturedName { get; private set; } = string.Empty;
    public string? CapturedSku { get; private set; }

    private LineItem() { }

    internal static LineItem Create(Guid orderId, Variant variant, int quantity)
    {
        return new LineItem
        {
            OrderId = orderId,
            VariantId = variant.Id,
            Quantity = quantity,
            Price = variant.Price,
            CapturedName = variant.Product?.Name ?? "Unknown Product",
            CapturedSku = variant.Sku
        };
    }

    internal void UpdateQuantity(int newQuantity)
    {
        Quantity = newQuantity;
    }
}