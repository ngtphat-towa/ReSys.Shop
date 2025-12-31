using FluentAssertions;
using NSubstitute;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.GetProductById;
using ReSys.Core.Interfaces;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new GetProductByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product" };
        
        // Mock FindAsync - Note: FindAsync takes params object[] keyValues
        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns(product);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
