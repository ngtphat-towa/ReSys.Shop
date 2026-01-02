using System.Text.RegularExpressions;

namespace ReSys.Core.Common.Helpers;

public static class NamingHelper
{
    private static readonly Regex ToSnakeCaseRegex1 = new(@"([A-Z]+)([A-Z][a-z])", RegexOptions.Compiled);
    private static readonly Regex ToSnakeCaseRegex2 = new(@"([a-z\d])([A-Z])", RegexOptions.Compiled);

    public static string ToSnakeCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        
        var result = ToSnakeCaseRegex1.Replace(str, "$1_$2");
        result = ToSnakeCaseRegex2.Replace(result, "$1_$2");
        
        return result.ToLowerInvariant();
    }

    public static string ToPascalCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        
        // Handle keys that don't need splitting (already plain words or single segment)
        if (!str.Contains('_'))
        {
            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }

        // Fix: preserve casing of subsequent letters to match MinPrice, PageSize, etc.
        return string.Join("", str.Split('_')
            .Select(s => s.Length > 0 ? char.ToUpperInvariant(s[0]) + s.Substring(1) : s));
    }
}
