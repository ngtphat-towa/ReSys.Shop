using FluentAssertions;
using NSubstitute;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.UpdateProductImage;
using ReSys.Core.Interfaces;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class UpdateProductImageCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMlService _mlService;
    private readonly UpdateProductImageCommandHandler _handler;

    public UpdateProductImageCommandHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _fileService = Substitute.For<IFileService>();
        _mlService = Substitute.For<IMlService>();
        _handler = new UpdateProductImageCommandHandler(_context, _fileService, _mlService);
    }

    [Fact]
    public async Task Handle_ShouldUpdateImage_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, ImageUrl = "old.jpg" };
        var stream = new MemoryStream();
        var fileName = "new.jpg";

        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns(product);
        
        _fileService.SaveFileAsync(stream, fileName, Arg.Any<CancellationToken>())
            .Returns("new_unique.jpg");

        // Act
        var command = new UpdateProductImageCommand(productId, stream, fileName);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ImageUrl.Should().Be("/api/files/new_unique.jpg");
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _context.Products.FindAsync(Arg.Is<object[]>(x => (Guid)x[0] == productId), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new UpdateProductImageCommand(productId, new MemoryStream(), "test.jpg");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
