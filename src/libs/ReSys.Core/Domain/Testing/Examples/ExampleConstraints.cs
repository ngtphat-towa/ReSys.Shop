namespace ReSys.Core.Domain.Testing.Examples;

public static class ExampleConstraints
{
    public const int NameMaxLength = 255;
    public const int DescriptionMaxLength = 2000;
    public const int HexColorMaxLength = 7;
    public const string HexColorRegex = "^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
    public const int PricePrecision = 18;
    public const int PriceScale = 2;
    public const decimal MinPrice = 0.01m;
}
