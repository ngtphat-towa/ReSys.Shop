using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Domain.Catalog.Products.Variants;

public sealed class Variant : Aggregate, IHasMetadata, ISoftDeletable
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

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

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
        if (price < VariantConstraints.MinPrice) return VariantErrors.InvalidPrice;
        if (sku?.Length > VariantConstraints.SkuMaxLength) return VariantErrors.SkuTooLong;

        var variant = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Sku = sku?.Trim(),
            Price = price,
            IsMaster = isMaster
        };

        variant.RaiseDomainEvent(new VariantEvents.VariantCreated(variant));
        return variant;
    }

    public ErrorOr<Success> UpdatePricing(decimal price, decimal? compareAtPrice = null)
    {
        if (price < VariantConstraints.MinPrice) return VariantErrors.InvalidPrice;
        if (compareAtPrice.HasValue && compareAtPrice.Value <= price)
            return VariantErrors.InvalidCompareAtPrice;

        Price = price;
        CompareAtPrice = compareAtPrice;

        RaiseDomainEvent(new VariantEvents.VariantPricingUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> UpdateDimensions(decimal? weight, decimal? height, decimal? width, decimal? depth)
    {
        if ((weight.HasValue && weight < VariantConstraints.Dimensions.MinValue) ||
            (height.HasValue && height < VariantConstraints.Dimensions.MinValue) ||
            (width.HasValue && width < VariantConstraints.Dimensions.MinValue) ||
            (depth.HasValue && depth < VariantConstraints.Dimensions.MinValue))
        {
            return VariantErrors.InvalidDimension;
        }

        Weight = weight;
        Height = height;
        Width = width;
        Depth = depth;

        RaiseDomainEvent(new VariantEvents.VariantDimensionsUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new VariantEvents.VariantDeleted(this));
        return Result.Deleted;
    }

    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;

        IsDeleted = false;
        DeletedAt = null;
        RaiseDomainEvent(new VariantEvents.VariantRestored(this));
        return Result.Success;
    }

    public ErrorOr<Success> AddOptionValue(OptionValue value)
    {
        if (IsMaster) return VariantErrors.MasterCannotHaveOptions;

        if (!OptionValues.Any(ov => ov.Id == value.Id))
        {
            OptionValues.Add(value);
            RaiseDomainEvent(new VariantEvents.VariantOptionValueAdded(this, value.Id));
        }
        return Result.Success;
    }

    public ErrorOr<Success> RemoveOptionValue(Guid valueId)
    {
        var value = OptionValues.FirstOrDefault(ov => ov.Id == valueId);
        if (value != null)
        {
            OptionValues.Remove(value);
            RaiseDomainEvent(new VariantEvents.VariantOptionValueRemoved(this, valueId));
        }
        return Result.Success;
    }
}
