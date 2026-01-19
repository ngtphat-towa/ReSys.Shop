namespace ReSys.Core.Common.Options.Systems;

/// <summary>
/// Storefront configuration options.
/// </summary>
public sealed class StorefrontOption : SystemOptionBase
{
    public const string Section = "Storefront";
    
    public StorefrontOption()
    {
        SystemName = "ReSys.Storefront";
        DefaultPage = "/home";
    }
}
