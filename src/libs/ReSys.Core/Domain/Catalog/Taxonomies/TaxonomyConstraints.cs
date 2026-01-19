using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Domain.Catalog.Taxonomies;

public static class TaxonomyConstraints
{
    public const int NameMaxLength = CommonConstraints.NameMaxLength;
    public const int PresentationMaxLength = CommonConstraints.PresentationMaxLength;
    public const int DefaultPosition = 0;
    public const int MinPosition = 0;
}
