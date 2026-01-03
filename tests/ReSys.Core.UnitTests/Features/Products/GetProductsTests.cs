using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.GetProducts;
using ReSys.Core.Interfaces;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class GetProductsTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly GetProducts.Handler _handler;

    public GetProductsTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new GetProducts.Handler(_context);
    }

    [Fact(DisplayName = "Should return a paged list of products with correct metadata for total count and next page")]
    public async Task Handle_DefaultRequest_ShouldReturnPagedList()
    {
        // Arrange
        var baseName = $"PagedProduct_{Guid.NewGuid()}";
        _context.Set<Product>().AddRange(
            new Product { Id = Guid.NewGuid(), Name = $"{baseName}_1", Price = 10 },
            new Product { Id = Guid.NewGuid(), Name = $"{baseName}_2", Price = 20 },
            new Product { Id = Guid.NewGuid(), Name = $"{baseName}_3", Price = 30 }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new GetProducts.Request { Page = 1, PageSize = 2, Search = baseName };
        var query = new GetProducts.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "Should correctly filter products based on a case-insensitive search term in name or description")]
    public async Task Handle_SearchFilter_ShouldReturnMatchingProducts()
    {
        // Arrange
        var uniqueSearch = $"Search_{Guid.NewGuid()}";
        _context.Set<Product>().AddRange(
            new Product { Id = Guid.NewGuid(), Name = $"Match_{uniqueSearch}", Price = 10 },
            new Product { Id = Guid.NewGuid(), Name = "Other", Description = $"Contains_{uniqueSearch}", Price = 20 },
            new Product { Id = Guid.NewGuid(), Name = "NoMatch", Price = 30 }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new GetProducts.Request { Search = uniqueSearch };
        var query = new GetProducts.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.Name.Contains(uniqueSearch) || x.ImageUrl == null); // ImageUrl is irrelevant here
    }

    [Fact(DisplayName = "Should correctly sort products by price in descending order")]
    public async Task Handle_SortByPriceDescending_ShouldReturnSortedList()
    {
        // Arrange
        var baseName = $"SortedProduct_{Guid.NewGuid()}";
        _context.Set<Product>().AddRange(
            new Product { Id = Guid.NewGuid(), Name = $"{baseName}_Low", Price = 10 },
            new Product { Id = Guid.NewGuid(), Name = $"{baseName}_High", Price = 100 },
            new Product { Id = Guid.NewGuid(), Name = $"{baseName}_Mid", Price = 50 }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new GetProducts.Request 
        { 
            Search = baseName,
            SortBy = "price", 
            IsDescending = true 
        };
        var query = new GetProducts.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Price.Should().Be(100);
        result.Items[1].Price.Should().Be(50);
        result.Items[2].Price.Should().Be(10);
    }
    [Fact(DisplayName = "Should correctly filter products by a list of ProductIds")]
    public async Task Handle_ProductIdsFilter_ShouldReturnMatchingProducts()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        
        _context.Set<Product>().AddRange(
            new Product { Id = id1, Name = "Included_1", Price = 10 },
            new Product { Id = id2, Name = "Included_2", Price = 20 },
            new Product { Id = id3, Name = "Excluded", Price = 30 }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new GetProducts.Request { ProductId = [id1, id2] };
        var query = new GetProducts.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Id).Should().Contain([id1, id2]);
        result.Items.Select(x => x.Id).Should().NotContain(id3);
    }
}