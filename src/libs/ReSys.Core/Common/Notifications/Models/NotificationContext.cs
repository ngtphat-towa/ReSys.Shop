using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Models;

/// <summary>
/// A refined container for notification template parameters stored as a list of key-value pairs.
/// </summary>
public sealed record NotificationContext
{
    public IReadOnlyList<NotificationParameter> Parameters { get; init; }

    internal NotificationContext(IEnumerable<NotificationParameter> parameters)
    {
        Parameters = parameters.ToList().AsReadOnly();
    }

    private NotificationContext()
    {
        Parameters = new List<NotificationParameter>().AsReadOnly();
    }

    public string? GetValue(NotificationConstants.Parameter parameter)
    {
        // Last-one-wins for duplicate keys
        return Parameters.LastOrDefault(p => p.Key == parameter)?.Value;
    }

    public static NotificationContext Empty => new();

    public static NotificationContext Create(params (NotificationConstants.Parameter Key, string? Value)[] items)
    {
        // Use grouping to ensure strictly unique keys in the initial list (last-one-wins)
        var list = items
            .GroupBy(i => i.Key)
            .Select(g => NotificationParameter.Create(g.Key, g.Last().Value))
            .ToList();

        return new NotificationContext(list);
    }

    public static NotificationContext ApplyParameter(NotificationContext context, NotificationConstants.Parameter key, string? value)
    {
        var newList = new List<NotificationParameter>(context.Parameters);

        // Remove existing to maintain a flat, unique-key list
        newList.RemoveAll(p => p.Key == key);
        newList.Add(NotificationParameter.Create(key, value));

        return new NotificationContext(newList);
    }
}
