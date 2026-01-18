namespace ReSys.Core.Domain.Catalog.Products.Variants;

public static class VariantConstraints
{
    public const int SkuMaxLength = 255;
    public const int BarcodeMaxLength = 100;
    
    public const int PricePrecision = 18;
    public const int PriceScale = 2;
    public const int DimensionPrecision = 18;
    public const int DimensionScale = 4;

    public const decimal MinPrice = 0;
    public const int DefaultPosition = 0;
    public const int MinPosition = 0;

    public static class Dimensions
    {
        public const decimal MinValue = 0;
    }

    public static class Units
    {
        public const int MaxLength = 20;
    }
}