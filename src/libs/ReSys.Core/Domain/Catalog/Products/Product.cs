using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Shared.Extensions;
using ErrorOr;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Products.Properties;

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
    public bool MarkedForRegenerateTaxonProducts { get; set; }
    
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
    public ICollection<Classification> Classifications { get; set; } = new List<Classification>();

    public Variant? MasterVariant => Variants.FirstOrDefault(v => v.IsMaster);

    public Product() { }

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
            Id = Guid.NewGuid(),
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

    public ErrorOr<Success> UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) return ProductErrors.NameRequired;
        if (name.Length > ProductConstraints.NameMaxLength) return ProductErrors.NameTooLong;

        Name = name.Trim();
        Presentation = name.Trim();
        Description = description?.Trim();
        
        RaiseDomainEvent(new ProductEvents.ProductUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        if (metaTitle?.Length > ProductConstraints.Seo.MetaTitleMaxLength) 
            return ProductErrors.Seo.MetaTitleTooLong;
        
        if (metaDescription?.Length > ProductConstraints.Seo.MetaDescriptionMaxLength) 
            return ProductErrors.Seo.MetaDescriptionTooLong;

        MetaTitle = metaTitle?.Trim();
        MetaDescription = metaDescription?.Trim();
        MetaKeywords = metaKeywords?.Trim();

        return Result.Success;
    }

    public ErrorOr<Success> SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return ProductErrors.InvalidSlug;
        
        Slug = slug.ToSlug();
        return Result.Success;
    }

    public ErrorOr<Success> SetPropertyValue(PropertyType propertyType, string value)
    {
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

    public void Archive()
    {
        if (Status == ProductStatus.Archived) return;
        Status = ProductStatus.Archived;
        RaiseDomainEvent(new ProductEvents.ProductArchived(this));
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProductEvents.ProductDeleted(this));
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAt = null;
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
        if (existing != null) existing.SetRole(ProductImage.ProductImageType.Gallery);
    }

    public enum ProductStatus { Draft, Active, Archived }
}