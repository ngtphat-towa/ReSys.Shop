using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Features.Products.DeleteProduct;
using ReSys.Core.Interfaces;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class DeleteProductTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly DeleteProduct.Handler _handler;

    public DeleteProductTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new DeleteProduct.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully delete an existing product from the database")]
    public async Task Handle_ExistingProduct_ShouldDelete()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product 
        { 
            Id = productId, 
            Name = $"DeleteMe_{Guid.NewGuid()}", 
            Description = "Desc",
            Price = 10 
        };
        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteProduct.Command(productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the product exists and can be deleted");
        result.Value.Should().Be(ErrorOr.Result.Deleted);

        var dbProduct = await _context.Set<Product>().FindAsync(productId);
        dbProduct.Should().BeNull("because the product was removed from the database");
    }

    [Fact(DisplayName = "Should return a not found error when attempting to delete a product that does not exist")]
    public async Task Handle_NonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteProduct.Command(nonExistentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because the product ID does not exist in the database");
        result.FirstError.Should().BeEquivalentTo(ProductErrors.NotFound(nonExistentId));
    }
}