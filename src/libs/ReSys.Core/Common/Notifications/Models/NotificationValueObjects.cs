using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Models;

public sealed record NotificationRecipient
{
    public string Identifier { get; init; }
    public string? Name { get; init; }

    private NotificationRecipient(string identifier, string? name = null)
    {
        Identifier = identifier;
        Name = name;
    }

    public static NotificationRecipient Create(string identifier, string? name = null) => new(identifier, name);
}

public sealed record NotificationParameter(NotificationConstants.Parameter Key, string? Value)
{
    public static NotificationParameter Create(NotificationConstants.Parameter key, string? value) => new(key, value);
}

public sealed record NotificationContent
{
    public string Subject { get; init; }
    public string Body { get; init; }
    public string? HtmlBody { get; init; }

    private NotificationContent(string subject, string body, string? htmlBody = null)
    {
        Subject = subject;
        Body = body;
        HtmlBody = htmlBody;
    }

    public static NotificationContent Create(string subject, string body, string? htmlBody = null)
        => new(subject, body, htmlBody);
}

public sealed record NotificationAttachment
{
    public string FileName { get; init; }
    public byte[] Data { get; init; }
    public string ContentType { get; init; }

    private NotificationAttachment(string fileName, byte[] data, string contentType)
    {
        FileName = fileName;
        Data = data;
        ContentType = contentType;
    }

    public static NotificationAttachment Create(string fileName, byte[] data, string contentType)
        => new(fileName, data, contentType);
}

public sealed record NotificationMetadata
{
    public NotificationConstants.PriorityLevel Priority { get; init; }
    public string Language { get; init; }
    public string CreatedBy { get; init; }

    private NotificationMetadata(
        NotificationConstants.PriorityLevel priority = NotificationConstants.PriorityLevel.Normal,
        string language = "en-US",
        string createdBy = "System")
    {
        Priority = priority;
        Language = language;
        CreatedBy = createdBy;
    }

    public static NotificationMetadata Create(
        NotificationConstants.PriorityLevel priority = NotificationConstants.PriorityLevel.Normal,
        string language = "en-US",
        string createdBy = "System")
        => new(priority, language, createdBy);

    public static NotificationMetadata Default => new();
}