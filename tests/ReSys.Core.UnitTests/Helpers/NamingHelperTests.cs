using ReSys.Core.Common.Helpers;

namespace ReSys.Core.UnitTests.Helpers;

public class NamingHelperTests
{
    [Theory(DisplayName = "ToSnakeCase: Should convert various input strings to snake_case format correctly")]
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
    public void ToSnakeCase_ShouldConvertHasExpected(string? input, string? expected)
    {
        var result = NamingHelper.ToSnakeCase(input!);
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "ToPascalCase: Should convert various input strings to PascalCase format correctly")]
    [InlineData("page_size", "PageSize")]
    [InlineData("min_price", "MinPrice")]
    [InlineData("search", "Search")] 
    [InlineData("Search", "Search")] 
    [InlineData("created_at_utc", "CreatedAtUtc")]
    [InlineData("id", "Id")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void ToPascalCase_ShouldConvertAsExpected(string? input, string? expected)
    {
        var result = NamingHelper.ToPascalCase(input!);
        result.Should().Be(expected);
    }
}
