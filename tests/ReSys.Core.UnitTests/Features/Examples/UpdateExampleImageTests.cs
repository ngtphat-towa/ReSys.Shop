using ErrorOr;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ReSys.Core.Common.AI;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Storage;
using ReSys.Core.Domain;
using ReSys.Core.Features.Examples.UpdateExampleImage;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

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

    [Fact(DisplayName = "Handle: Should successfully update the image and its embedding when the example exists")]
    public async Task Handle_ExistingExample_ShouldUpdateImageAndEmbedding()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var example = new Example { Id = exampleId, Name = "Test", ImageUrl = "old.jpg" };
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(CancellationToken.None);

        var stream = new MemoryStream();
        var fileName = "new.jpg";
        var resultFileName = "saved_new.jpg";
        
        var uploadResult = new FileUploadResult(resultFileName, resultFileName, fileName, 100, "image/jpeg", "hash", "temp", DateTime.UtcNow);
        _fileService.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mlService.GetEmbeddingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()) 
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        var request = new UpdateExampleImage.Request(exampleId, stream, fileName);
        var command = new UpdateExampleImage.Command(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse("because the example exists and services are operational");
        result.Value.ImageUrl.Should().Be($"/api/files/{resultFileName}");

        var dbExample = await _context.Set<Example>().Include(p => p.Embedding).FirstOrDefaultAsync(p => p.Id == exampleId);
        dbExample!.ImageUrl.Should().Be($"/api/files/{resultFileName}");
        dbExample.Embedding.Should().NotBeNull("because the ML service returned a valid embedding");
    }
}
