namespace ReSys.Core.Features.Products.Common;

public static class ProductConstraints
{
    public const int NameMaxLength = 255;
    public const int PricePrecision = 18;
    public const int PriceScale = 2;
    public const decimal MinPrice = 0.01m;
}
