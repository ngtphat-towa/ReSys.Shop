using ReSys.Core.Features.Testing.Examples.Common;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Testing.Examples;

[Collection("Shared Database")]
public class GetExamplesTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/testing/examples: Should support pagination (page and page_size)")]
    public async Task Get_WithPagination_ReturnsCorrectSlice()
    {
        var uniquePrefix = $"Pagination_{Guid.NewGuid()}";
        await SeedExamplesAsync(15, uniquePrefix);

        var response = await Client.GetAsync($"/api/testing/examples?search={uniquePrefix}&page=2&page_size=5", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(5);
        apiResponse.Meta!.TotalCount.Should().Be(15);
        apiResponse.Meta.Page.Should().Be(2);
    }

    [Fact(DisplayName = "GET /api/testing/examples: Should support price range filtering (min_price and max_price)")]
    public async Task Get_WithPriceFilter_ReturnsFilteredResults()
    {
        var uniquePrefix = $"Price_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_Cheap", 10);
        await SeedExampleAsync($"{uniquePrefix}_Mid", 50);
        await SeedExampleAsync($"{uniquePrefix}_Expensive", 100);

        var response = await Client.GetAsync($"/api/testing/examples?search={uniquePrefix}&min_price=40&max_price=60", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(1);
        apiResponse.Data!.First().Name.Should().Be($"{uniquePrefix}_Mid");
    }

    [Fact(DisplayName = "GET /api/testing/examples: Should support complex sorting and searching combined (sort_by and is_descending)")]
    public async Task Get_WithSearchAndSort_ReturnsMatchingSortedResults()
    {
        var uniquePrefix = $"SearchSort_{Guid.NewGuid()}";
        await SeedExampleAsync($"{uniquePrefix}_Apple iPhone", 1000);
        await SeedExampleAsync($"{uniquePrefix}_Apple iPad", 800);
        await SeedExampleAsync($"{uniquePrefix}_Samsung Galaxy", 900);

        var response = await Client.GetAsync($"/api/testing/examples?search={uniquePrefix}_Apple&sort_by=price&is_descending=false", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
        apiResponse.Data![0].Name.Should().Be($"{uniquePrefix}_Apple iPad"); 
        apiResponse.Data[1].Name.Should().Be($"{uniquePrefix}_Apple iPhone");
    }

    [Fact(DisplayName = "GET /api/testing/examples: Should support filtering by created_from using ISO 8601 DateTimeOffset")]
    public async Task Get_WithCreatedFromFilter_ReturnsMatchingResults()
    {
        var uniquePrefix = $"DateFilter_{Guid.NewGuid()}";
        var oldDate = DateTimeOffset.UtcNow.AddDays(-10);
        var newDate = DateTimeOffset.UtcNow;
        
        await SeedExampleWithDateAsync($"{uniquePrefix}_Old", 10, oldDate);
        await SeedExampleWithDateAsync($"{uniquePrefix}_New", 20, newDate);
        
        var filterDate = DateTimeOffset.UtcNow.AddDays(-5).ToString("yyyy-MM-ddTHH:mm:ssZ");

        var response = await Client.GetAsync($"/api/testing/examples?search={uniquePrefix}&created_from={filterDate}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(1);
        apiResponse.Data!.First().Name.Should().Be($"{uniquePrefix}_New");
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
            ImageUrl = "" // Required by non-nullable
        });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SeedExampleAsync(string name, decimal price)
    {
        Context.Set<Example>().Add(new Example { Id = Guid.NewGuid(), Name = name, Description = "D", Price = price, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SeedExamplesAsync(int count, string prefix)
    {
        for (int i = 1; i <= count; i++)
            Context.Set<Example>().Add(new Example { Id = Guid.NewGuid(), Name = $"{prefix}_{i}", Description = "D", Price = i, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact(DisplayName = "GET /api/testing/examples: Should support filtering by multiple Example_ids")]
    public async Task Get_WithExampleIdsFilter_ReturnsMatchingResults()
    {
        var uniquePrefix = $"IdsFilter_{Guid.NewGuid()}";
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        Context.Set<Example>().AddRange(
            new Example { Id = id1, Name = $"{uniquePrefix}_1", Description = "D", Price = 10, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" },
            new Example { Id = id2, Name = $"{uniquePrefix}_2", Description = "D", Price = 20, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" },
            new Example { Id = id3, Name = $"{uniquePrefix}_3", Description = "D", Price = 30, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" }
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Query string: Example_id=id1&Example_id=id2
        var response = await Client.GetAsync($"/api/testing/examples?Example_id={id1}&Example_id={id2}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
        var items = apiResponse.Data!;
        items.Select(x => x.Id).Should().Contain([id1, id2]);
        items.Select(x => x.Id).Should().NotContain(id3);
    }

    [Fact(DisplayName = "GET /api/testing/examples: Should support multi-status filtering")]
    public async Task Get_WithMultiStatusFilter_ReturnsMatchingResults()
    {
        var uniquePrefix = $"StatusFilter_{Guid.NewGuid()}";
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        Context.Set<Example>().AddRange(
            new Example { Id = id1, Name = $"{uniquePrefix}_1", Status = ExampleStatus.Active, Price = 10, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" },
            new Example { Id = id2, Name = $"{uniquePrefix}_2", Status = ExampleStatus.Draft, Price = 20, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" },
            new Example { Id = id3, Name = $"{uniquePrefix}_3", Status = ExampleStatus.Archived, Price = 30, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" }
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Query string: status=0&status=1 (Draft and Active)
        var response = await Client.GetAsync($"/api/testing/examples?status={(int)ExampleStatus.Draft}&status={(int)ExampleStatus.Active}&search={uniquePrefix}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
        apiResponse.Data!.Select(x => x.Status).Should().Contain([ExampleStatus.Draft, ExampleStatus.Active]);
        apiResponse.Data!.Select(x => x.Status).Should().NotContain(ExampleStatus.Archived);
    }

    [Fact(DisplayName = "GET /api/testing/examples: Should return empty data when page index exceeds total pages")]
    public async Task Get_PageExceedingTotal_ReturnsEmptyData()
    {
        var uniquePrefix = $"Exceed_{Guid.NewGuid()}";
        await SeedExamplesAsync(5, uniquePrefix);

        // Page 10 with size 10 -> empty
        var response = await Client.GetAsync($"/api/testing/examples?search={uniquePrefix}&page=10&page_size=10", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().BeEmpty();
        apiResponse.Meta!.TotalCount.Should().Be(5);
    }

    [Fact(DisplayName = "GET /api/testing/examples: Should return empty data when search term has no matches")]
    public async Task Get_NoSearchMatches_ReturnsEmptyData()
    {
        var response = await Client.GetAsync($"/api/testing/examples?search=NonExistentTerm_{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().BeEmpty();
        apiResponse.Meta!.TotalCount.Should().Be(0);
    }
}


