using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Features.Products.GetProductById;
using ReSys.Core.Interfaces;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class GetProductByIdTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly GetProductById.Handler _handler;

    public GetProductByIdTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new GetProductById.Handler(_context);
    }

    [Fact(DisplayName = "Should return the correct product details when a product with the specified ID exists")]
    public async Task Handle_ExistingProduct_ShouldReturnDetails()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product 
        { 
            Id = productId, 
            Name = $"TestProduct_{Guid.NewGuid()}",
            Description = "Full Description",
            Price = 49.99m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new GetProductById.Request(productId);
        var query = new GetProductById.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the product exists");
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(productId);
        result.Value.Name.Should().Be(product.Name);
    }

    [Fact(DisplayName = "Should return a not found error when searching for a product ID that does not exist")]
    public async Task Handle_NonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetProductById.Request(nonExistentId);
        var query = new GetProductById.Query(request);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because no product exists with the given ID");
        result.FirstError.Should().BeEquivalentTo(ProductErrors.NotFound(nonExistentId));
    }
}
