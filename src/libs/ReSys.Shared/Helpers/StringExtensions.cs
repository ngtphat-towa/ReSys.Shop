namespace ReSys.Shared.Helpers;

public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        return NamingHelper.ToSnakeCase(str);
    }

    public static string ToPascalCase(this string str)
    {
        return NamingHelper.ToPascalCase(str);
    }
}
