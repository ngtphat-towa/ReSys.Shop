using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.UnitTests.Common.Notifications.Models;

public class NotificationContextTests
{
    [Fact(DisplayName = "Create should populate Parameters list correctly")]
    public void Create_ShouldPopulateParameters()
    {
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.UserFirstName, "Alice"),
            (NotificationConstants.Parameter.OrderId, "ORD-123")
        );

        context.Parameters.Should().HaveCount(2);
        context.Parameters.Should().Contain(p => p.Key == NotificationConstants.Parameter.UserFirstName && p.Value == "Alice");
        context.Parameters.Should().Contain(p => p.Key == NotificationConstants.Parameter.OrderId && p.Value == "ORD-123");
    }

    [Fact(DisplayName = "GetValue should return the correct value")]
    public void GetValue_ShouldReturnCorrectValue()
    {
        var context = NotificationContext.Create((NotificationConstants.Parameter.UserFirstName, "Alice"));
        context.GetValue(NotificationConstants.Parameter.UserFirstName).Should().Be("Alice");
    }

    [Fact(DisplayName = "GetValue should return null for missing parameter")]
    public void GetValue_Missing_ShouldReturnNull()
    {
        var context = NotificationContext.Empty;
        context.GetValue(NotificationConstants.Parameter.UserFirstName).Should().BeNull();
    }

    [Fact(DisplayName = "ApplyParameter should update existing parameter in list")]
    public void ApplyParameter_Update_ShouldRemoveOldAndAddNew()
    {
        var context = NotificationContext.Create((NotificationConstants.Parameter.UserFirstName, "Old"));
        var updated = NotificationContext.ApplyParameter(context, NotificationConstants.Parameter.UserFirstName, "New");

        updated.GetValue(NotificationConstants.Parameter.UserFirstName).Should().Be("New");
        updated.Parameters.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Create should handle duplicate keys by taking the last value")]
    public void Create_DuplicateKeys_ShouldTakeLast()
    {
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.UserFirstName, "First"),
            (NotificationConstants.Parameter.UserFirstName, "Last")
        );

        context.GetValue(NotificationConstants.Parameter.UserFirstName).Should().Be("Last");
        context.Parameters.Count(p => p.Key == NotificationConstants.Parameter.UserFirstName).Should().Be(1);
    }
}
