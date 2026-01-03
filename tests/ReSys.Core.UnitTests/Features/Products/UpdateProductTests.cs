using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Features.Products.UpdateProduct;
using ReSys.Core.Interfaces;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class UpdateProductTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly UpdateProduct.Handler _handler;

    public UpdateProductTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new UpdateProduct.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully update an existing product with new details when the request is valid")]
    public async Task Handle_ValidRequest_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialName = $"OldName_{Guid.NewGuid()}";
        var product = new Product 
        { 
            Id = productId, 
            Name = initialName, 
            Description = "Old Desc",
            Price = 10,
            ImageUrl = "old.jpg"
        };
        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync(CancellationToken.None);

        var newName = $"NewName_{Guid.NewGuid()}";
        var request = new UpdateProduct.Request
        {
            Name = newName,
            Description = "New Desc",
            Price = 20,
            ImageUrl = "new.jpg"
        };
        var command = new UpdateProduct.Command(productId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the request is valid and name is changed to something unique");
        result.Value.Name.Should().Be(newName);
        result.Value.Price.Should().Be(20);

        var dbProduct = await _context.Set<Product>().FindAsync(productId);
        dbProduct!.Name.Should().Be(newName);
    }

    [Fact(DisplayName = "Should return a conflict error when updating a product name to one that already exists for another product")]
    public async Task Handle_NameConflict_ShouldReturnConflict()
    {
        // Arrange
        var existingName = $"Conflict_{Guid.NewGuid()}";
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        
        _context.Set<Product>().AddRange(
            new Product { Id = product1Id, Name = existingName, Description = "D1", Price = 10 },
            new Product { Id = product2Id, Name = "UniqueName", Description = "D2", Price = 20 }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new UpdateProduct.Request { Name = existingName, Description = "Changed", Price = 20 };
        var command = new UpdateProduct.Command(product2Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because the name already belongs to another product");
        result.FirstError.Should().BeEquivalentTo(ProductErrors.DuplicateName);
    }

    [Fact(DisplayName = "Should return a not found error when attempting to update a product that does not exist")]
    public async Task Handle_NonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateProduct.Request { Name = "Valid", Description = "Desc", Price = 10 };
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateProduct.Command(nonExistentId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because the product ID does not exist");
        result.FirstError.Should().BeEquivalentTo(ProductErrors.NotFound(nonExistentId));
    }
}
