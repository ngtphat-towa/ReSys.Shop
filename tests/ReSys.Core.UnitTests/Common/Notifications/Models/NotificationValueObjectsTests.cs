using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.UnitTests.Common.Notifications.Models;

public class NotificationValueObjectsTests
{
    [Fact(DisplayName = "NotificationRecipient.Create should set properties")]
    public void NotificationRecipient_Create_ShouldWork()
    {
        var result = NotificationRecipient.Create("test@test.com", "Name");
        result.Identifier.Should().Be("test@test.com");
        result.Name.Should().Be("Name");
    }

    [Fact(DisplayName = "NotificationContent.Create should set properties")]
    public void NotificationContent_Create_ShouldWork()
    {
        var result = NotificationContent.Create("S", "B", "H");
        result.Subject.Should().Be("S");
        result.Body.Should().Be("B");
        result.HtmlBody.Should().Be("H");
    }

    [Fact(DisplayName = "NotificationAttachment.Create should set properties")]
    public void NotificationAttachment_Create_ShouldWork()
    {
        byte[] data = [1, 2, 3];
        var result = NotificationAttachment.Create("file.txt", data, "text/plain");
        result.FileName.Should().Be("file.txt");
        result.Data.Should().BeEquivalentTo(data);
        result.ContentType.Should().Be("text/plain");
    }

    [Fact(DisplayName = "NotificationMetadata.Create should set properties")]
    public void NotificationMetadata_Create_ShouldWork()
    {
        var result = NotificationMetadata.Create(NotificationConstants.PriorityLevel.Low, "fr-FR", "Tester");
        result.Priority.Should().Be(NotificationConstants.PriorityLevel.Low);
        result.Language.Should().Be("fr-FR");
        result.CreatedBy.Should().Be("Tester");
    }

    [Fact(DisplayName = "NotificationMetadata.Default should have expected defaults")]
    public void NotificationMetadata_Default_ShouldBeCorrect()
    {
        var result = NotificationMetadata.Default;
        result.Priority.Should().Be(NotificationConstants.PriorityLevel.Normal);
        result.Language.Should().Be("en-US");
        result.CreatedBy.Should().Be("System");
    }
}
