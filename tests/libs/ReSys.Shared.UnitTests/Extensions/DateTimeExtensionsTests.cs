using ReSys.Shared.Extensions;

namespace ReSys.Shared.UnitTests.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class DateTimeExtensionsTests
{
    [Fact(DisplayName = "FormatUtc: Should return ISO 8601 string for DateTimeOffset")]
    public void FormatUtc_ShouldReturnIso8601String()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.FromHours(2));
        // UTC: 2024-01-15T08:30:00Z

        // Act
        var result = date.FormatUtc();

        // Assert
        result.Should().Be("2024-01-15T08:30:00Z");
    }

    [Fact(DisplayName = "FormatUtc: Should handle nullable DateTimeOffset correctly")]
    public void FormatUtc_Nullable_ShouldHandleNull()
    {
        DateTimeOffset? date = null;
        date.FormatUtc().Should().BeNull();
    }
}