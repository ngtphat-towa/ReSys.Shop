namespace ReSys.Core.Domain.Ordering;

/// <summary>
/// Defines validation boundaries and financial constants for the Order domain.
/// </summary>
public static class OrderConstraints
{
    public const int NumberMaxLength = 50;
    public const int EmailMaxLength = 255;
    public const int CurrencyCodeLength = 3;
    public const int SpecialInstructionsMaxLength = 1000;
    public const int PromoCodeMaxLength = 50;

    /// <summary>Format for human-readable order identifiers.</summary>
    public const string NumberPrefix = "ORD-";

    /// <summary>Precision for all monetary values in the order domain (Cents).</summary>
    public const int FinancialPrecision = 18;
    public const int FinancialScale = 2;
}
