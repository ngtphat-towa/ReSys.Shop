namespace ReSys.Core.Domain.Settings.ShippingMethods;

public static class ShippingMethodConstraints
{
    public const int NameMaxLength = 100;
    public const int PresentationMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    
    public const decimal MinCost = 0;
    public const decimal MaxCost = 1_000_000;

    public const string NumberPrefix = "SHP-";
}