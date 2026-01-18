namespace ReSys.Core.Domain.Catalog.Products;

public static class ProductConstraints
{
    public const int NameMaxLength = 255;
    public const int PresentationMaxLength = 255;
    public const int SlugMaxLength = 255;
    public const int DescriptionMaxLength = 5000;

    public const decimal MinPrice = 0;
    public const int DefaultPosition = 0;
    public const int MinPosition = 0;
    
    // SEO Constraints
    public static class Seo
    {
        public const int MetaTitleMaxLength = 70;
        public const int MetaDescriptionMaxLength = 160;
        public const int MetaKeywordsMaxLength = 255;
    }
}
