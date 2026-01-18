namespace ReSys.Core.Features.Catalog.Products.Classifications.Common;

public record ProductClassificationParameters
{
    public Guid TaxonId { get; set; }
    public int Position { get; set; }
}

public record ProductClassificationListItem : ProductClassificationParameters
{
    public Guid Id { get; set; }
    public string? TaxonName { get; set; }
    public string? TaxonPrettyName { get; set; }
}
