namespace ReSys.Shared.Extensions;

public static class EnumerableExtensions
{
    public static string JoinToSentence(this IEnumerable<string> items)
    {
        var list = items.ToList();

        if (list.Count == 0)
            return string.Empty;

        if (list.Count == 1)
            return list[0];

        if (list.Count == 2)
            return $"{list[0]} and {list[1]}";

        return $"{string.Join(", ", list.Take(list.Count - 1))}, and {list[^1]}";
    }
}
