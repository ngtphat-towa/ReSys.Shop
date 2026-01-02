using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.UpdateProductImage;
using ReSys.Core.Interfaces;
using ReSys.Core.UnitTests.Common;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Products;

public class UpdateProductImageTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMlService _mlService;
    private readonly UpdateProductImage.Handler _handler;

    public UpdateProductImageTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _fileService = Substitute.For<IFileService>();
        _mlService = Substitute.For<IMlService>();
        _handler = new UpdateProductImage.Handler(_context, _fileService, _mlService);
    }

    [Fact(DisplayName = "Should successfully update product image URL and generate a new embedding when an existing product receives a new image")]
    public async Task Handle_ExistingProduct_ShouldUpdateImageAndEmbedding()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = $"ImageTest_{Guid.NewGuid()}", Description = "D", Price = 10 };
        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync(CancellationToken.None);

        var stream = new MemoryStream();
        var request = new UpdateProductImage.Request(productId, stream, "test.jpg");
        var command = new UpdateProductImage.Command(request);

        _fileService.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("saved_test.jpg");

        _mlService.GetEmbeddingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the product exists and services are operational");
        result.Value.ImageUrl.Should().Be("/api/files/saved_test.jpg");

        var dbProduct = await _context.Set<Product>().Include(p => p.Embedding).FirstOrDefaultAsync(p => p.Id == productId);
        dbProduct!.ImageUrl.Should().Be("/api/files/saved_test.jpg");
        dbProduct.Embedding.Should().NotBeNull("because the ML service returned a valid embedding");
    }
}