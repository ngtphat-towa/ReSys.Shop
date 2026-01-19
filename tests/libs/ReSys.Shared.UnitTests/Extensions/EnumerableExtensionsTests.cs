using ReSys.Shared.Extensions;

namespace ReSys.Shared.UnitTests.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class EnumerableExtensionsTests
{
    [Fact(DisplayName = "JoinToSentence: Should return empty string for empty collection")]
    public void JoinToSentence_Empty_ReturnsEmpty()
    {
        Enumerable.Empty<string>().JoinToSentence().Should().BeEmpty();
    }

    [Fact(DisplayName = "JoinToSentence: Should return the single item for collection with one item")]
    public void JoinToSentence_OneItem_ReturnsItem()
    {
        new[] { "Apple" }.JoinToSentence().Should().Be("Apple");
    }

    [Fact(DisplayName = "JoinToSentence: Should join two items with 'and'")]
    public void JoinToSentence_TwoItems_ReturnsItemsJoinedByAnd()
    {
        new[] { "Apple", "Banana" }.JoinToSentence().Should().Be("Apple and Banana");
    }

    [Fact(DisplayName = "JoinToSentence: Should join multiple items with commas and 'and'")]
    public void JoinToSentence_ThreeItems_ReturnsItemsJoinedByCommaAndAnd()
    {
        new[] { "Apple", "Banana", "Cherry" }.JoinToSentence().Should().Be("Apple, Banana, and Cherry");
    }
}
