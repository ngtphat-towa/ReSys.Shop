namespace ReSys.Core.Domain.Settings.Stores;

public static class SettingKey
{
    public static string Store(StoreSettingKey key)
        => $"Store.{key}";

    public static string Seo(SeoSettingKey key)
        => $"Seo.{key}";

    public static string Checkout(CheckoutSettingKey key)
        => $"Checkout.{key}";

    public static string Email(EmailSettingKey key)
        => $"Email.{key}";

    public static string Inventory(InventorySettingKey key)
        => $"Inventory.{key}";
}
