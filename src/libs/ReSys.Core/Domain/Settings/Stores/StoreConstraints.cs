namespace ReSys.Core.Domain.Settings.Stores;

public static class StoreConstraints
{
    public const int NameMaxLength = 100;
    public const int CodeMaxLength = 50;
    public const int UrlMaxLength = 255;
    public const int CurrencyCodeMaxLength = 3; // ISO 4217
    public const int WeightUnitMaxLength = 10; 

    // Format: Uppercase alphanumeric with underscores (e.g., "MAIN_US")
    public const string CodeRegex = "^[A-Z0-9_]+$";
}
