using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Domain.Catalog.Products.Variants;

public sealed class Variant : Entity, IHasMetadata
{
    public Guid ProductId { get; set; }
    public bool IsMaster { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    
    // Physical
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    
    // Pricing
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    
    public bool TrackInventory { get; set; } = true;
    public int Position { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Relationships
    public Product Product { get; set; } = null!;
    public ICollection<OptionValue> OptionValues { get; set; } = new List<OptionValue>();
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();

    private Variant() { }

    public static ErrorOr<Variant> Create(
        Guid productId, 
        string sku, 
        decimal price, 
        bool isMaster = false)
    {
        if (price < 0) return VariantErrors.InvalidPrice;

        return new Variant
        {
            ProductId = productId,
            Sku = sku?.Trim(),
            Price = price,
            IsMaster = isMaster
        };
    }

    public ErrorOr<Success> UpdatePricing(decimal price, decimal? compareAtPrice = null)
    {
        if (price < 0) return VariantErrors.InvalidPrice;
        if (compareAtPrice.HasValue && compareAtPrice.Value <= price)
            return Error.Validation("Variant.InvalidCompareAtPrice", "Compare-at price must be greater than current price.");

        Price = price;
        CompareAtPrice = compareAtPrice;
        return Result.Success;
    }

    public void UpdateDimensions(decimal? weight, decimal? height, decimal? width, decimal? depth)
    {
        Weight = weight;
        Height = height;
        Width = width;
        Depth = depth;
    }

    public void AddOptionValue(OptionValue value)
    {
        if (!OptionValues.Any(ov => ov.Id == value.Id))
        {
            OptionValues.Add(value);
        }
    }

    public void RemoveOptionValue(Guid valueId)
    {
        var value = OptionValues.FirstOrDefault(ov => ov.Id == valueId);
        if (value != null)
        {
            OptionValues.Remove(value);
        }
    }
}