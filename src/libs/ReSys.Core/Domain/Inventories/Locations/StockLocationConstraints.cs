namespace ReSys.Core.Domain.Inventories.Locations;

public static class StockLocationConstraints
{
    public const int NameMaxLength = 255;
    public const int PresentationMaxLength = 255;
    public const int CodeMinLength = 2;
    public const int CodeMaxLength = 50;

    // Format: alphanumeric, hyphens, and underscores only (ERP Standard)
    public const string CodeRegex = "^[A-Z0-9_-]+$";
}
