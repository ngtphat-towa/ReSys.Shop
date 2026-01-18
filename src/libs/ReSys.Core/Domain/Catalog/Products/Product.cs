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
    public DateTimeOffset? DiscontinuedOn { get; set; }
    public DateTimeOffset? MakeActiveAt { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
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
        string? description = null)
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

        if (metaKeywords?.Length > ProductConstraints.Seo.MetaKeywordsMaxLength)
            return ProductErrors.Seo.MetaKeywordsTooLong;

        MetaTitle = metaTitle?.Trim();
        MetaDescription = metaDescription?.Trim();
        MetaKeywords = metaKeywords?.Trim();

        return Result.Success;
    }

    public ErrorOr<Success> SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return ProductErrors.InvalidSlug;
        if (slug.Length > ProductConstraints.SlugMaxLength) return ProductErrors.InvalidSlug;

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

    public ErrorOr<Success> RemoveProperty(Guid propertyTypeId)
    {
        var property = ProductProperties.FirstOrDefault(p => p.PropertyTypeId == propertyTypeId);
        if (property != null)
        {
            ProductProperties.Remove(property);
        }
        return Result.Success;
    }

    public ErrorOr<Success> Activate()
    {
        if (Status == ProductStatus.Active) return ProductErrors.Status.AlreadyActive;

        var oldStatus = Status;
        Status = ProductStatus.Active;
        AvailableOn ??= DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ProductEvents.ProductStatusChanged(this, oldStatus, Status));
        return Result.Success;
    }

    public ErrorOr<Success> Archive()
    {
        if (Status == ProductStatus.Archived) return ProductErrors.Status.AlreadyArchived;

        var oldStatus = Status;
        Status = ProductStatus.Archived;

        RaiseDomainEvent(new ProductEvents.ProductStatusChanged(this, oldStatus, Status));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ProductEvents.ProductDeleted(this));
        return Result.Deleted;
    }

    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;
        IsDeleted = false;
        DeletedAt = null;
        RaiseDomainEvent(new ProductEvents.ProductRestored(this));
        return Result.Success;
    }

    public ErrorOr<Variant> AddVariant(string sku, decimal price)
    {
        if (Variants.Any(v => v.Sku == sku))
            return ProductErrors.DuplicateSku;

        var variantResult = Variant.Create(Id, sku, price);
        if (variantResult.IsError) return variantResult.Errors;

        Variants.Add(variantResult.Value);
        RaiseDomainEvent(new ProductEvents.VariantAdded(this, variantResult.Value));
        return variantResult.Value;
    }

    public ErrorOr<Deleted> RemoveVariant(Guid variantId)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            return VariantErrors.NotFound(variantId);

        if (variant.IsMaster)
            return ProductErrors.CannotDeleteMasterVariant;

        variant.Delete();
        RaiseDomainEvent(new ProductEvents.VariantRemoved(this, variant));
        return Result.Deleted;
    }

    public ErrorOr<Success> RestoreVariant(Guid variantId)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            return VariantErrors.NotFound(variantId);

        variant.Restore();
        RaiseDomainEvent(new ProductEvents.VariantRestored(this, variant));
        return Result.Success;
    }

    public ErrorOr<Success> SetMasterVariant(Guid variantId)
    {
        var targetVariant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (targetVariant == null)
            return VariantErrors.NotFound(variantId);

        if (targetVariant.IsMaster)
            return Result.Success;

        foreach (var variant in Variants)
        {
            variant.IsMaster = (variant.Id == variantId);
        }

        // Master variant typically doesn't have specific option values 
        targetVariant.OptionValues.Clear();

        RaiseDomainEvent(new ProductEvents.ProductUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> AddClassification(Guid taxonId, int position = 0)
    {
        if (Classifications.Any(c => c.TaxonId == taxonId))
            return ProductErrors.DuplicateClassification;

        var classificationResult = Classification.Create(Id, taxonId, position);
        if (classificationResult.IsError) return classificationResult.Errors;

        Classifications.Add(classificationResult.Value);
        MarkedForRegenerateTaxonProducts = true;
        RaiseDomainEvent(new ProductEvents.ClassificationAdded(this, classificationResult.Value));
        return Result.Success;
    }

    public ErrorOr<Success> UpdateClassificationPosition(Guid taxonId, int position)
    {
        var classification = Classifications.FirstOrDefault(c => c.TaxonId == taxonId);
        if (classification == null)
            return ProductErrors.ClassificationNotFound;

        classification.UpdatePosition(position);
        MarkedForRegenerateTaxonProducts = true;
        return Result.Success;
    }

    public ErrorOr<Success> RemoveClassification(Guid taxonId)
    {
        var classification = Classifications.FirstOrDefault(c => c.TaxonId == taxonId);
        if (classification == null)
            return ProductErrors.ClassificationNotFound;

        Classifications.Remove(classification);
        MarkedForRegenerateTaxonProducts = true;
        RaiseDomainEvent(new ProductEvents.ClassificationRemoved(this, classification));
        return Result.Success;
    }

    public ErrorOr<Success> AddOptionType(OptionType optionType)
    {
        if (OptionTypes.Any(ot => ot.Id == optionType.Id))
            return Error.Conflict("Product.DuplicateOptionType", "This option type is already associated with the product.");

        OptionTypes.Add(optionType);
        RaiseDomainEvent(new ProductEvents.ProductUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> RemoveOptionType(Guid optionTypeId)
    {
        var optionType = OptionTypes.FirstOrDefault(ot => ot.Id == optionTypeId);
        if (optionType == null)
            return Error.NotFound("Product.OptionTypeNotFound", "Option type not found for this product.");

        OptionTypes.Remove(optionType);
        RaiseDomainEvent(new ProductEvents.ProductUpdated(this));
        return Result.Success;
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

    public ErrorOr<Success> UpdateImage(Guid imageId, string? alt, int position, ProductImage.ProductImageType role)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            return Error.NotFound("Product.ImageNotFound", "Product image not found.");

        if (role != image.Role)
        {
            if (role == ProductImage.ProductImageType.Default)
            {
                DemoteExistingRole(ProductImage.ProductImageType.Default);
            }
            else if (role == ProductImage.ProductImageType.Search)
            {
                DemoteExistingRole(ProductImage.ProductImageType.Search);
            }
            image.SetRole(role);
        }

        image.Update(alt, position);
        RaiseDomainEvent(new ProductEvents.ProductUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> RemoveImage(Guid imageId)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            return Error.NotFound("Product.ImageNotFound", "Product image not found.");

        Images.Remove(image);
        RaiseDomainEvent(new ProductEvents.ImageRemoved(this, image));
        return Result.Success;
    }

    private void DemoteExistingRole(ProductImage.ProductImageType roleToDemote)
    {
        var existing = Images.FirstOrDefault(i => i.Role == roleToDemote);
        if (existing != null) existing.SetRole(ProductImage.ProductImageType.Gallery);
    }

    public enum ProductStatus { Draft, Active, Archived }
}
