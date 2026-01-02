using FluentAssertions;
using ReSys.Core.Common.Models;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using System.Net.Http.Json;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class GetProductsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "GET /api/products: Should support pagination (page and page_size)")]
    public async Task Get_WithPagination_ReturnsCorrectSlice()
    {
        var uniquePrefix = $"Pagination_{Guid.NewGuid()}";
        await SeedProductsAsync(15, uniquePrefix);

        var response = await Client.GetAsync($"/api/products?search={uniquePrefix}&page=2&page_size=5");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductListItem>>(JsonOptions);
        result!.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(2);
    }

    [Fact(DisplayName = "GET /api/products: Should support price range filtering (min_price and max_price)")]
    public async Task Get_WithPriceFilter_ReturnsFilteredResults()
    {
        var uniquePrefix = $"Price_{Guid.NewGuid()}";
        await SeedProductAsync($"{uniquePrefix}_Cheap", 10);
        await SeedProductAsync($"{uniquePrefix}_Mid", 50);
        await SeedProductAsync($"{uniquePrefix}_Expensive", 100);

        var response = await Client.GetAsync($"/api/products?search={uniquePrefix}&min_price=40&max_price=60");

        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductListItem>>(JsonOptions);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be($"{uniquePrefix}_Mid");
    }

    [Fact(DisplayName = "GET /api/products: Should support complex sorting and searching combined (sort_by and is_descending)")]
    public async Task Get_WithSearchAndSort_ReturnsMatchingSortedResults()
    {
        var uniquePrefix = $"SearchSort_{Guid.NewGuid()}";
        await SeedProductAsync($"{uniquePrefix}_Apple iPhone", 1000);
        await SeedProductAsync($"{uniquePrefix}_Apple iPad", 800);
        await SeedProductAsync($"{uniquePrefix}_Samsung Galaxy", 900);

        var response = await Client.GetAsync($"/api/products?search={uniquePrefix}_Apple&sort_by=price&is_descending=false");

        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductListItem>>(JsonOptions);
        result!.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be($"{uniquePrefix}_Apple iPad"); 
        result.Items[1].Name.Should().Be($"{uniquePrefix}_Apple iPhone");
    }

    [Fact(DisplayName = "GET /api/products: Should support filtering by created_from using ISO 8601 DateTimeOffset")]
    public async Task Get_WithCreatedFromFilter_ReturnsMatchingResults()
    {
        var uniquePrefix = $"DateFilter_{Guid.NewGuid()}";
        var oldDate = DateTimeOffset.UtcNow.AddDays(-10);
        var newDate = DateTimeOffset.UtcNow;
        
        await SeedProductWithDateAsync($"{uniquePrefix}_Old", 10, oldDate);
        await SeedProductWithDateAsync($"{uniquePrefix}_New", 20, newDate);
        
        var filterDate = DateTimeOffset.UtcNow.AddDays(-5).ToString("yyyy-MM-ddTHH:mm:ssZ");

        var response = await Client.GetAsync($"/api/products?search={uniquePrefix}&created_from={filterDate}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductListItem>>(JsonOptions);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be($"{uniquePrefix}_New");
    }

    private async Task SeedProductWithDateAsync(string name, decimal price, DateTimeOffset createdAt)
    {
        Context.Set<Product>().Add(new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = name, 
            Description = "D", 
            Price = price,
            CreatedAt = createdAt
        });
        await Context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task SeedProductAsync(string name, decimal price)
    {
        Context.Set<Product>().Add(new Product { Id = Guid.NewGuid(), Name = name, Description = "D", Price = price, CreatedAt = DateTimeOffset.UtcNow });
        await Context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task SeedProductsAsync(int count, string prefix)
    {
        for (int i = 1; i <= count; i++)
            Context.Set<Product>().Add(new Product { Id = Guid.NewGuid(), Name = $"{prefix}_{i}", Description = "D", Price = i, CreatedAt = DateTimeOffset.UtcNow });
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}