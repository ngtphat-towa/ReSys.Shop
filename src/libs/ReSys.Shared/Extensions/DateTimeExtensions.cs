namespace ReSys.Shared.Extensions;

public static class DateTimeExtensions
{
    public static string? FormatUtc(this DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    public static string FormatUtc(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
