using System.Text;

namespace ReSys.Core.Common.Helpers;

public static class NamingHelper
{
    public static string ToSnakeCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        // Estimate capacity: original length + 20% for underscores is usually enough
        var builder = new StringBuilder(str.Length + (str.Length / 5));
        var state = SnakeCaseState.Start;

        ReadOnlySpan<char> span = str.AsSpan();

        for (int i = 0; i < span.Length; i++)
        {
            char current = span[i];

            if (current == ' ')
            {
                if (state != SnakeCaseState.Start)
                {
                    state = SnakeCaseState.NewWord;
                }
            }
            else if (char.IsUpper(current))
            {
                switch (state)
                {
                    case SnakeCaseState.Upper:
                        bool hasNext = (i + 1 < span.Length);
                        if (i > 0 && hasNext)
                        {
                            char next = span[i + 1];
                            if (!char.IsUpper(next) && next != '_')
                            {
                                builder.Append('_');
                            }
                        }
                        break;
                    case SnakeCaseState.Lower:
                    case SnakeCaseState.NewWord:
                        builder.Append('_');
                        break;
                }

                builder.Append(char.ToLowerInvariant(current));
                state = SnakeCaseState.Upper;
            }
            else if (current == '_')
            {
                builder.Append('_');
                state = SnakeCaseState.Start;
            }
            else
            {
                if (state == SnakeCaseState.NewWord)
                {
                    builder.Append('_');
                }

                builder.Append(current);
                state = SnakeCaseState.Lower;
            }
        }

        return builder.ToString();
    }

    private enum SnakeCaseState
    {
        Start,
        Lower,
        Upper,
        NewWord
    }

    public static string ToPascalCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        // Optimization: If no underscores
        if (!str.Contains('_'))
        {
            // If already capitalized, return original reference to avoid allocation
            if (char.IsUpper(str[0]))
            {
                return str;
            }
            // Otherwise standardize to Pascal (e.g. "search" -> "Search")
            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }

        var builder = new StringBuilder(str.Length);
        bool newWord = true;
        
        ReadOnlySpan<char> span = str.AsSpan();

        foreach (char c in span)
        {
            if (c == '_')
            {
                newWord = true;
                continue;
            }

            if (newWord)
            {
                builder.Append(char.ToUpperInvariant(c));
                newWord = false;
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
