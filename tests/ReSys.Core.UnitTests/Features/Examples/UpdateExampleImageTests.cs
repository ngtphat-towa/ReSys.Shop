using ReSys.Core.Features.Examples.UpdateExampleImage;

namespace ReSys.Core.UnitTests.Features.Examples;

public class UpdateExampleImageTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMlService _mlService;
    private readonly UpdateExampleImage.Handler _handler;

    public UpdateExampleImageTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _fileService = Substitute.For<IFileService>();
        _mlService = Substitute.For<IMlService>();
        _handler = new UpdateExampleImage.Handler(_context, _fileService, _mlService);
    }

    [Fact(DisplayName = "Should successfully update example image URL and generate a new embedding when an existing example receives a new image")]
    public async Task Handle_ExistingExample_ShouldUpdateImageAndEmbedding()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var example = new Example { Id = exampleId, Name = $"ImageTest_{Guid.NewGuid()}", Description = "D", Price = 10 };
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(CancellationToken.None);

        var stream = new MemoryStream();
        var request = new UpdateExampleImage.Request(exampleId, stream, "test.jpg");
        var command = new UpdateExampleImage.Command(request);

        _fileService.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>()) 
            .Returns("saved_test.jpg");

        _mlService.GetEmbeddingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()) 
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the example exists and services are operational");
        result.Value.ImageUrl.Should().Be("/api/files/saved_test.jpg");

        var dbExample = await _context.Set<Example>().Include(p => p.Embedding).FirstOrDefaultAsync(p => p.Id == exampleId);
        dbExample!.ImageUrl.Should().Be("/api/files/saved_test.jpg");
        dbExample.Embedding.Should().NotBeNull("because the ML service returned a valid embedding");
    }
}