using ReSys.Core.Features.Testing.Examples.Common;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Testing.Examples;

[Collection("Shared Database")]
public class GetExamplesV2Tests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support dynamic filtering (price range)")]
    public async Task Get_WithDynamicFilter_ReturnsFilteredResults()
    {
        var uniquePrefix = $"V2Price_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_Cheap", 10);
        await SeedExampleAsync($"{uniquePrefix}_Mid", 50);
        await SeedExampleAsync($"{uniquePrefix}_Expensive", 100);

        var filter = $"Name*{uniquePrefix},Price>=40,Price<=60";
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter={filter}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(1);
        apiResponse.Data!.First().Name.Should().Be($"{uniquePrefix}_Mid");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support complex OR grouping in filters")]
    public async Task Get_WithComplexGroupedFilter_ReturnsCorrectItems()
    {
        var uniquePrefix = $"V2Group_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_A", 10);
        await SeedExampleAsync($"{uniquePrefix}_B", 100);
        await SeedExampleAsync("Other", 1000); 

        var filter = $"Name*{uniquePrefix},(Price<20|Price>500)";
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter={filter}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(1);
        apiResponse.Data!.First().Name.Should().Be($"{uniquePrefix}_A");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support multi-field sorting")]
    public async Task Get_WithMultiSort_ReturnsCorrectOrder()
    {
        var uniquePrefix = $"V2MultiSort_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_Z", 10);
        await SeedExampleAsync($"{uniquePrefix}_A", 10);
        await SeedExampleAsync($"{uniquePrefix}_M", 50);

        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Name*{uniquePrefix}&sort=Price,Name", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(3);
        apiResponse.Data![0].Name.Should().Be($"{uniquePrefix}_A");
        apiResponse.Data![1].Name.Should().Be($"{uniquePrefix}_Z");
        apiResponse.Data![2].Name.Should().Be($"{uniquePrefix}_M");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support global search with corrected search_field")]
    public async Task Get_WithGlobalSearch_ReturnsMatchingResults()
    {
        var uniquePrefix = $"V2SearchFixed_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_Match", 10);
        await SeedExampleAsync($"{uniquePrefix}_Other", 20);

        // search_field (singular) should bind correctly now
        var response = await Client.GetAsync($"/api/testing/examples/v2?search={uniquePrefix}_Match&search_field=Name", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(1);
        apiResponse.Data!.First().Name.Should().Be($"{uniquePrefix}_Match");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support snake_case property filtering")]
    public async Task Get_WithSnakeCasePropertyFilter_ReturnsCorrectItems()
    {
        var uniquePrefix = $"V2Snake_{Guid.NewGuid()}";
        // Use a fixed date and query with exact string representation
        var date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await SeedExampleWithDateAsync($"{uniquePrefix}_Item", 10, date);

        // created_at -> CreatedAt. Use >= to handle potential precision/offset minor diffs or string parsing defaults
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=created_at>=2025-01-01,name*{uniquePrefix}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().Contain(x => x.Name == $"{uniquePrefix}_Item");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support pagination metadata")]
    public async Task Get_WithPagination_ReturnsCorrectMetadata()
    {
        var uniquePrefix = $"V2Pager_{Guid.NewGuid()}";
        for(int i=1; i<=15; i++) await SeedExampleAsync($"{uniquePrefix}_{i:D2}", i);

        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Name*{uniquePrefix}&page=2&page_size=5&sort=Name", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(5);
        apiResponse.Meta!.TotalCount.Should().Be(15);
        apiResponse.Meta.Page.Should().Be(2);
        apiResponse.Meta.PageSize.Should().Be(5);
        apiResponse.Data![0].Name.Should().Be($"{uniquePrefix}_06");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should handle malformed filter string gracefully")]
    public async Task Get_WithMalformedFilter_ReturnsAll()
    {
        var uniquePrefix = $"V2Mal_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_1", 10);

        // Missing operator/value
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Name", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().Contain(x => x.Name == $"{uniquePrefix}_1");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should support mixed sort direction")]
    public async Task Get_WithMixedSort_ReturnsCorrectOrder()
    {
        var uniquePrefix = $"V2Mixed_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_A", 10);
        await SeedExampleAsync($"{uniquePrefix}_B", 10);
        await SeedExampleAsync($"{uniquePrefix}_C", 20);

        // Price DESC, Name ASC
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Name*{uniquePrefix}&sort=Price desc, Name asc", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(3);
        apiResponse.Data![0].Name.Should().Be($"{uniquePrefix}_C");
        apiResponse.Data[1].Name.Should().Be($"{uniquePrefix}_A");
        apiResponse.Data[2].Name.Should().Be($"{uniquePrefix}_B");
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should default to page 1 for negative page index")]
    public async Task Get_NegativePage_ReturnsFirstPage()
    {
        var uniquePrefix = $"V2Neg_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_1", 10);

        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Name*{uniquePrefix}&page=-5", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Meta!.Page.Should().Be(1);
        apiResponse.Data.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should filter by nested category name")]
    public async Task Get_NestedCategoryFilter_ReturnsCorrectItems()
    {
        var categoryName = $"NestedCat_{Guid.NewGuid()}";
        var cat = new ExampleCategory { Id = Guid.NewGuid(), Name = categoryName };
        Context.Set<ExampleCategory>().Add(cat);
        
        var exampleName = $"NestedItem_{Guid.NewGuid()}";
        await SeedExampleWithCategoryAsync(exampleName, 10, cat.Id);
        await SeedExampleAsync($"Other_{Guid.NewGuid()}", 20);

        // Filter: Category.Name=...
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Category.Name={categoryName}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(1);
        apiResponse.Data![0].Name.Should().Be(exampleName);
        apiResponse.Data[0].CategoryName.Should().Be(categoryName);
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should filter by nested category name using snake_case")]
    public async Task Get_NestedSnakeCategoryFilter_ReturnsCorrectItems()
    {
        var categoryName = $"SnakeCat_{Guid.NewGuid()}";
        var cat = new ExampleCategory { Id = Guid.NewGuid(), Name = categoryName };
        Context.Set<ExampleCategory>().Add(cat);
        
        var exampleName = $"SnakeNestedItem_{Guid.NewGuid()}";
        await SeedExampleWithCategoryAsync(exampleName, 10, cat.Id);

        // Filter: category.name=...
        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=category.name={categoryName}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().Contain(x => x.Name == exampleName);
    }

    [Fact(DisplayName = "GET /api/testing/examples/v2: Should sort by nested category name")]
    public async Task Get_NestedCategorySort_ReturnsCorrectOrder()
    {
        var catA = new ExampleCategory { Id = Guid.NewGuid(), Name = "A_IntegrationCat" };
        var catB = new ExampleCategory { Id = Guid.NewGuid(), Name = "B_IntegrationCat" };
        Context.Set<ExampleCategory>().AddRange(catA, catB);
        
        var uniquePrefix = $"NestedSort_{Guid.NewGuid()}";
        await SeedExampleWithCategoryAsync($"{uniquePrefix}_1", 10, catB.Id);
        await SeedExampleWithCategoryAsync($"{uniquePrefix}_2", 10, catA.Id);

        var response = await Client.GetAsync($"/api/testing/examples/v2?filter=Name*{uniquePrefix}&sort=Category.Name", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
        apiResponse.Data![0].CategoryName.Should().Be("A_IntegrationCat");
        apiResponse.Data[1].CategoryName.Should().Be("B_IntegrationCat");
    }

    private async Task SeedExampleWithCategoryAsync(string name, decimal price, Guid categoryId)
    {
        Context.Set<Example>().Add(new Example 
        { 
            Id = Guid.NewGuid(), 
            Name = name, 
            Description = "D", 
            Price = price, 
            CreatedAt = DateTimeOffset.UtcNow, 
            ImageUrl = "",
            CategoryId = categoryId
        });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SeedExampleWithDateAsync(string name, decimal price, DateTimeOffset createdAt)
    {
        Context.Set<Example>().Add(new Example 
        { 
            Id = Guid.NewGuid(), 
            Name = name, 
            Description = "D", 
            Price = price,
            CreatedAt = createdAt,
            ImageUrl = "" 
        });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SeedExampleAsync(string name, decimal price)
    {
        Context.Set<Example>().Add(new Example { Id = Guid.NewGuid(), Name = name, Description = "D", Price = price, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
