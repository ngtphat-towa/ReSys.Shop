using System.Text.RegularExpressions;

namespace ReSys.Core.Common.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex SnakeCaseRegex();

    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return SnakeCaseRegex().Replace(str, "$1_$2").ToLower();
    }

    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        
        // Handle snake_case input (e.g., "created_at" -> "CreatedAt")
        if (str.Contains('_'))
        {
            var parts = str.Split('_', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    var chars = parts[i].ToCharArray();
                    chars[0] = char.ToUpperInvariant(chars[0]);
                    parts[i] = new string(chars);
                }
            }
            return string.Join("", parts);
        }

        // Handle camelCase or normal string
        var firstChar = char.ToUpperInvariant(str[0]);
        return firstChar + str.Substring(1);
    }
}
