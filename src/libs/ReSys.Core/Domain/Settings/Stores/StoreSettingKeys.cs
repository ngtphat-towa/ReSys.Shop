namespace ReSys.Core.Domain.Settings.Stores;

public enum StoreSettingKey
{
    Name,
    Code,
    DefaultCurrency,
    SupportedCurrencies,
    DefaultLocale,
    SupportedLocales,
    MailFromAddress,
    SupportEmail,
    Address,
    ContactPhone
}

public enum SeoSettingKey
{
    Title,
    MetaDescription,
    MetaKeywords
}

public enum CheckoutSettingKey
{
    ZoneId,
    AllowedCountries
}

public enum EmailSettingKey
{
    NewOrderNotificationEmail
}

public enum InventorySettingKey
{
    LowStockThreshold
}
