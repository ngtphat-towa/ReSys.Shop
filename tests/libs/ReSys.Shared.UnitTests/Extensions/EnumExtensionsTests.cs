using System.ComponentModel;


using ReSys.Shared.Extensions;

namespace ReSys.Shared.UnitTests.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class EnumExtensionsTests
{
    private enum TestEnum
    {
        [Description("First Value")]
        First,
        Second
    }

    [Fact(DisplayName = "GetDescription: Should return DescriptionAttribute value when present")]
    public void GetDescription_ShouldReturnDescriptionAttribute_WhenPresent()
    {
        TestEnum.First.GetDescription().Should().Be("First Value");
    }

    [Fact(DisplayName = "GetDescription: Should return string representation when attribute is missing")]
    public void GetDescription_ShouldReturnStringRepresentation_WhenAttributeMissing()
    {
        TestEnum.Second.GetDescription().Should().Be("Second");
    }

    [Fact(DisplayName = "GetDescriptions: Should return all descriptions for an enum type")]
    public void GetDescriptions_ShouldReturnAllDescriptions()
    {
        var descriptions = EnumExtensions.GetDescriptions<TestEnum>();
        descriptions.Should().ContainInOrder("First Value", "Second");
    }
}
