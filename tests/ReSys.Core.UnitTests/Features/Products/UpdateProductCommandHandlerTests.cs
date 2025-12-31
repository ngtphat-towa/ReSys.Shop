using FluentAssertions;
using NSubstitute;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.UpdateProduct;
using ReSys.Core.Interfaces;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _fileService = Substitute.For<IFileService>();
        _handler = new UpdateProductCommandHandler(_context, _fileService);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product 
        { 
            Id = productId, 
            Name = "Old Name", 
            Description = "Old Desc", 
            Price = 10 
        };

        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            productId, 
            "New Name", 
            "New Desc", 
            20, 
            null, 
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("New Name");
        result.Value.Price.Should().Be(20);
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateImage_WhenImageIsProvided()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, ImageUrl = "old.jpg" };
        var stream = new MemoryStream();
        var fileName = "new.jpg";

        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns(product);
        
        _fileService.SaveFileAsync(stream, fileName, Arg.Any<CancellationToken>())
            .Returns("path/to/new.jpg");

        var command = new UpdateProductCommand(
            productId, 
            "Name", 
            "Desc", 
            10, 
            stream, 
            fileName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ImageUrl.Should().Be("path/to/new.jpg");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new UpdateProductCommand(productId, "Name", "Desc", 10, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
