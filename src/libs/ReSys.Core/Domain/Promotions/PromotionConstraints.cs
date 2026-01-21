namespace ReSys.Core.Domain.Promotions;

/// <summary>
/// Defines architectural constraints for the Promotions module.
/// </summary>
public static class PromotionConstraints
{
    public const int NameMaxLength = 100;
    public const int CodeMaxLength = 50;
    public const int DescriptionMaxLength = 1000;

    public const decimal MinOrderAmount = 0m;
    public const decimal MaxOrderAmount = 1_000_000m;

    public const decimal MinDiscountValue = 0m;
    public const decimal MaxDiscountValue = 1_000_000m;

    public const int MinUsageLimit = 1;

    public static class Actions
    {
        public const int MinQuantity = 1;
        public const int MaxQuantity = 1000;
    }
}
