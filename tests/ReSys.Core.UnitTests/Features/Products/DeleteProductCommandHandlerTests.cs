using FluentAssertions;
using NSubstitute;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.DeleteProduct;
using ReSys.Core.Interfaces;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new DeleteProductCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId };

        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new DeleteProductCommand(productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(ErrorOr.Result.Deleted);
        _context.Products.Received(1).Remove(product);
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new DeleteProductCommand(productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
        _context.Products.DidNotReceiveWithAnyArgs().Remove(Arg.Any<Product>());
    }
}
