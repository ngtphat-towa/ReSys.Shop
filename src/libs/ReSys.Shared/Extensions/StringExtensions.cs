using System.Text;
using Humanizer;
using Slugify;

namespace ReSys.Shared.Extensions;

public static class StringExtensions
{
    private static readonly SlugHelper SlugHelper = new();

    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var builder = new StringBuilder(str.Length + (str.Length / 5));
        var state = SnakeCaseState.Start;

        ReadOnlySpan<char> span = str.AsSpan();

        for (int i = 0; i < span.Length; i++)
        {
            char current = span[i];

            if (current == ' ' || current == '_' || current == '.')
            {
                if (state != SnakeCaseState.Start && state != SnakeCaseState.NewWord)
                {
                    state = SnakeCaseState.NewWord;
                }

                if (current == '.')
                {
                    builder.Append('.');
                    state = SnakeCaseState.Start;
                }
                continue;
            }

            if (char.IsUpper(current))
            {
                switch (state)
                {
                    case SnakeCaseState.Upper:
                        bool hasNext = (i + 1 < span.Length);
                        if (i > 0 && hasNext)
                        {
                            char next = span[i + 1];
                            if (!char.IsUpper(next) && next != '_' && next != ' ' && next != '.')
                            {
                                builder.Append('_');
                            }
                        }
                        break;
                    case SnakeCaseState.Lower:
                    case SnakeCaseState.NewWord:
                        builder.Append('_');
                        break;
                    case SnakeCaseState.Start:
                        break;
                }

                builder.Append(char.ToLowerInvariant(current));
                state = SnakeCaseState.Upper;
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

    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var builder = new StringBuilder(str.Length);
        bool newWord = true;

        ReadOnlySpan<char> span = str.AsSpan();

        foreach (char c in span)
        {
            if (c == '_' || c == ' ' || c == '.')
            {
                newWord = true;
                if (c == '.') builder.Append('.');
                continue;
            }

            if (newWord)
            {
                builder.Append(char.ToUpperInvariant(c));
                newWord = false;
            }
            else
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }

        return builder.ToString();
    }

    public static string ToHumanize(this string? str)
    {
        return string.IsNullOrWhiteSpace(str) ? string.Empty : str.Humanize();
    }

    public static string ToSlug(this string? str)
    {
        return string.IsNullOrWhiteSpace(str) ? string.Empty : SlugHelper.GenerateSlug(str);
    }

    private enum SnakeCaseState
    {
        Start,
        Lower,
        Upper,
        NewWord
    }
}
