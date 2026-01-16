using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Shared.Extensions;
using ErrorOr;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.Domain.Catalog.Products;

public sealed class Product : Aggregate, ISoftDeletable, IHasSlug, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTimeOffset? AvailableOn { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public bool IsDigital { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Collections
    public ICollection<Variant> Variants { get; set; } = new List<Variant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<OptionType> OptionTypes { get; set; } = new List<OptionType>();
    public ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>();
    public ICollection<Taxon> Taxons { get; set; } = new List<Taxon>();

    public Variant? MasterVariant => Variants.FirstOrDefault(v => v.IsMaster);

    private Product() { }

    public static ErrorOr<Product> Create(
        string name, 
        string sku, 
        decimal price, 
        string? slug = null, 
        string? description = null,
        bool isDigital = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return ProductErrors.NameRequired;
        if (name.Length > ProductConstraints.NameMaxLength) return ProductErrors.NameTooLong;

        var product = new Product
        {
            Name = name.Trim(),
            Presentation = name.Trim(),
            Slug = string.IsNullOrWhiteSpace(slug) ? name.ToSlug() : slug.ToSlug(),
            Description = description?.Trim(),
            IsDigital = isDigital,
            Status = ProductStatus.Draft
        };

        var masterVariantResult = Variant.Create(product.Id, sku, price, isMaster: true);
        if (masterVariantResult.IsError) return masterVariantResult.Errors;
        
        product.Variants.Add(masterVariantResult.Value);

        product.RaiseDomainEvent(new ProductEvents.ProductCreated(product));
        return product;
    }

    public ErrorOr<Success> SetPropertyValue(PropertyType propertyType, string value)
    {
        // Business Rule: Ensure the property type is compatible or at least registered
        var existing = ProductProperties.FirstOrDefault(p => p.PropertyTypeId == propertyType.Id);
        if (existing != null)
        {
            existing.UpdateValue(value);
        }
        else
        {
            ProductProperties.Add(ProductProperty.Create(Id, propertyType.Id, value));
        }
        return Result.Success;
    }

    public void RemoveProperty(Guid propertyTypeId)
    {
        var property = ProductProperties.FirstOrDefault(p => p.PropertyTypeId == propertyTypeId);
        if (property != null)
        {
            ProductProperties.Remove(property);
        }
    }

    public void Activate()
    {
        if (Status == ProductStatus.Active) return;
        Status = ProductStatus.Active;
        AvailableOn ??= DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProductEvents.ProductActivated(this));
    }

    public ErrorOr<Variant> AddVariant(string sku, decimal price)
    {
        if (Variants.Any(v => v.Sku == sku))
            return Error.Conflict("Product.DuplicateSku", $"Variant with SKU {sku} already exists for this product.");

        var variantResult = Variant.Create(Id, sku, price);
        if (variantResult.IsError) return variantResult.Errors;

        Variants.Add(variantResult.Value);
        RaiseDomainEvent(new ProductEvents.VariantAdded(this, variantResult.Value));
        return variantResult.Value;
    }

    public ErrorOr<ProductImage> AddImage(string url, string? alt = null, Guid? variantId = null, ProductImage.ProductImageType role = ProductImage.ProductImageType.Gallery)
    {
        if (!Images.Any() && role == ProductImage.ProductImageType.Gallery)
        {
            role = ProductImage.ProductImageType.Default;
        }

        if (role == ProductImage.ProductImageType.Default)
        {
            DemoteExistingRole(ProductImage.ProductImageType.Default);
        }
        else if (role == ProductImage.ProductImageType.Search)
        {
            DemoteExistingRole(ProductImage.ProductImageType.Search);
        }

        var imageResult = ProductImage.Create(Id, url, alt, variantId, role: role);
        if (imageResult.IsError) return imageResult.Errors;

        Images.Add(imageResult.Value);
        RaiseDomainEvent(new ProductEvents.ImageAdded(this, imageResult.Value));
        return imageResult.Value;
    }

    private void DemoteExistingRole(ProductImage.ProductImageType roleToDemote)
    {
        var existing = Images.FirstOrDefault(i => i.Role == roleToDemote);
        if (existing != null) existing.Role = ProductImage.ProductImageType.Gallery;
    }

    public enum ProductStatus { Draft, Active, Archived }
}