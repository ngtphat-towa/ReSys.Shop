namespace ReSys.Core.Features.Admin.Catalog.Products.Properties.Common;

public record ProductPropertyParameters
{
    public Guid PropertyTypeId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public record ProductPropertyListItem : ProductPropertyParameters
{
    public Guid Id { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public string PropertyTypePresentation { get; set; } = string.Empty;
}
