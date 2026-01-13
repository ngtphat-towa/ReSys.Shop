using System.Text;

namespace ReSys.Core.Common.Helpers;

public static class NamingHelper
{
    public static string ToSnakeCase(string str)
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
                        // Don't add underscore at the very beginning
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
}