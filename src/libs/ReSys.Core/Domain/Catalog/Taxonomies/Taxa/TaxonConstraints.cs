using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

public static class TaxonConstraints
{
    public const int NameMaxLength = CommonConstraints.NameMaxLength;
    public const int PresentationMaxLength = CommonConstraints.PresentationMaxLength;
    public const int PermalinkMaxLength = CommonConstraints.SlugMaxLength;
    public const int DefaultPosition = 0;
    public const int MinPosition = 0;
}