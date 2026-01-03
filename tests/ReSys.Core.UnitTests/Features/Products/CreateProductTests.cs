using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Features.Products.CreateProduct;
using ReSys.Core.Interfaces;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class CreateProductTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly CreateProduct.Handler _handler;

    public CreateProductTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new CreateProduct.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully create a product and save it to the database when the request is valid")]
    public async Task Handle_ValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var uniqueName = $"Product_{Guid.NewGuid()}";
        var request = new CreateProduct.Request
        {
            Name = uniqueName,
            Description = "A valid product description",
            Price = 99.99m,
            ImageUrl = "http://example.com/image.jpg"
        };
        var command = new CreateProduct.Command(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the request is valid and the name is unique");
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(uniqueName);

        var dbProduct = await _context.Set<Product>().FindAsync(result.Value.Id);
        dbProduct.Should().NotBeNull("because the product should be persisted in the database");
    }

    [Fact(DisplayName = "Should return a conflict error when attempting to create a product with a name that already exists")]
    public async Task Handle_DuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var duplicateName = $"Duplicate_{Guid.NewGuid()}";
        _context.Set<Product>().Add(new Product { Id = Guid.NewGuid(), Name = duplicateName, Description = "Existing", Price = 10 });
        await _context.SaveChangesAsync(CancellationToken.None);

        var request = new CreateProduct.Request { Name = duplicateName, Description = "New", Price = 20 };
        var command = new CreateProduct.Command(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue("because a product with the same name already exists");
        result.FirstError.Should().BeEquivalentTo(ProductErrors.DuplicateName);
    }
}
