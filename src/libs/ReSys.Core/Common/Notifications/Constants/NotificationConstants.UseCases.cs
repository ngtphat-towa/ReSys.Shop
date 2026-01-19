namespace ReSys.Core.Common.Notifications.Constants;

public static partial class NotificationConstants
{
    public enum UseCase
    {
        None = 0,

        // System notifications
        SystemActiveEmail,
        SystemActivePhone,
        SystemResetPassword,
        System2FaOtp,
        SystemOrderConfirmation,
        SystemOrderShipped,
        SystemOrderFailed,
        SystemAccountUpdate,
        SystemPromotionEmail,

        // User notifications
        UserWelcomeEmail,
        UserProfileUpdateEmail,
        UserPasswordChangeNotification,

        // Payment notifications
        PaymentSuccessEmail,
        PaymentFailureEmail,
        PaymentRefundNotification,

        // Marketing notifications
        MarketingNewsletter,
        MarketingDiscountOffer,
        MarketingSurvey,

        // Fashion-specific notifications
        NewCollectionLaunch,
        FlashSaleNotification,
        BackInStockNotification,
        LoyaltyRewardEarned,
        AbandonedCartReminder,
        WishlistItemOnSale
    }

}
