using ReSys.Shared.Extensions;

namespace ReSys.Shared.UnitTests.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class StringExtensionsTests
{
    [Theory(DisplayName = "ToSnakeCase: Should convert various input strings correctly")]
    [InlineData("PageSize", "page_size")]
    [InlineData("MinPrice", "min_price")]
    [InlineData("start_date", "start_date")] 
    [InlineData("Simple", "simple")]
    [InlineData("Description", "description")]
    [InlineData("_description", "description")]
    [InlineData("request.Description", "request.description")]
    [InlineData("Unparseable123", "unparseable123")]
    [InlineData("Already_Snake", "already_snake")]
    [InlineData("HTMLHandler", "html_handler")] 
    [InlineData("XMLParser", "xml_parser")]
    public void ToSnakeCase_ShouldReturnSnakeCase(string input, string expected)
    {
        input.ToSnakeCase().Should().Be(expected);
    }

    [Theory(DisplayName = "ToPascalCase: Should convert various input strings correctly")]
    [InlineData("page_size", "PageSize")]
    [InlineData("min_price", "MinPrice")]
    [InlineData("search", "Search")] 
    [InlineData("Search", "Search")] 
    [InlineData("created_at_utc", "CreatedAtUtc")]
    [InlineData("id", "Id")]
    [InlineData("", "")]
    public void ToPascalCase_ShouldReturnPascalCase(string input, string expected)
    {
        input.ToPascalCase().Should().Be(expected);
    }

    [Theory(DisplayName = "ToHumanize: Should convert string to human-friendly format")]
    [InlineData("OptionType", "Option type")]
    [InlineData("TodoList", "Todo list")]
    public void ToHumanize_ShouldReturnHumanFriendlyString(string input, string expected)
    {
        input.ToHumanize().Should().Be(expected);
    }

    [Theory(DisplayName = "ToSlug: Should convert string to URL-friendly slug")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Product Name 123", "product-name-123")]
    public void ToSlug_ShouldReturnUrlFriendlyString(string input, string expected)
    {
        input.ToSlug().Should().Be(expected);
    }
}
