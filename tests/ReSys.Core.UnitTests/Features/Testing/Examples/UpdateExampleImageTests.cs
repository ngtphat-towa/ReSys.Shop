using Microsoft.EntityFrameworkCore;

using NSubstitute;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Storage;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.UpdateExampleImage;
using ReSys.Core.UnitTests.TestInfrastructure;

using ReSys.Core.Common.Imaging;
using ReSys.Core.Common.Ml;

namespace ReSys.Core.UnitTests.Features.Testing.Examples;

public class UpdateExampleImageTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMlService _mlService;
    private readonly IImageService _imageService;
    private readonly UpdateExampleImage.Handler _handler;

    public UpdateExampleImageTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _fileService = Substitute.For<IFileService>();
        _mlService = Substitute.For<IMlService>();
        _imageService = Substitute.For<IImageService>();
        _handler = new UpdateExampleImage.Handler(_context, _fileService, _mlService, _imageService);
    }

    [Fact(DisplayName = "Handle: Should successfully update the image and its embedding when the example exists")]
    public async Task Handle_ExistingExample_ShouldUpdateImageAndEmbedding()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var example = new Example { Id = exampleId, Name = "Test", ImageUrl = "/api/files/old.jpg" };
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stream = new MemoryStream();
        var fileName = "new.jpg";
        var resultFileName = "saved_new.webp";
        
        // Mock Image Processing
        var processedImage = new ProcessedImageResult(
            Main: new ImageVariant(new MemoryStream(), resultFileName, 800, 600, 1000),
            Thumbnail: null,
            Responsive: new List<ImageVariant>(),
            OriginalWidth: 1000,
            OriginalHeight: 1000
        );

        _imageService.ProcessAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(processedImage);

        var uploadResult = new FileUploadResult(resultFileName, resultFileName, resultFileName, 100, "image/webp", "hash", "examples", DateTime.UtcNow);
        _fileService.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileUploadOptions>(), Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mlService.GetEmbeddingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()) 
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        var request = new UpdateExampleImage.Request(exampleId, stream, fileName);
        var command = new UpdateExampleImage.Command(request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse("because the example exists and services are operational");
        result.Value.ImageUrl.Should().Be($"/api/files/examples/{resultFileName}");

        var dbExample = await _context.Set<Example>().Include(p => p.Embedding).FirstOrDefaultAsync(p => p.Id == exampleId, cancellationToken: TestContext.Current.CancellationToken);
        dbExample!.ImageUrl.Should().Be($"/api/files/examples/{resultFileName}");
        dbExample.Embedding.Should().NotBeNull("because the ML service returned a valid embedding");
        
        // Verify old file was deleted
        await _fileService.Received(1).DeleteFileAsync("old.jpg", Arg.Any<CancellationToken>());
    }
}

