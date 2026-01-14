using ErrorOr;

using MediatR;

using ReSys.Core.Common.Data;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Common.Storage;
using ReSys.Core.Common.Imaging;
using ReSys.Core.Common.Ml;
using ReSys.Core.Domain.Testing.Examples;

namespace ReSys.Core.Features.Testing.Examples.UpdateExampleImage;

public static class UpdateExampleImage
{
    public class Request(Guid id, Stream imageStream, string imageName)
    {
        public Guid Id { get; set; } = id;
        public Stream ImageStream { get; set; } = imageStream;
        public string ImageName { get; set; } = imageName;
    }

    public record Command(Request Request) : IRequest<ErrorOr<ExampleDetail>>;

    public class Handler(
        IApplicationDbContext context,
        IFileService fileService,
        IMlService mlService,
        IImageService imageService) : IRequestHandler<Command, ErrorOr<ExampleDetail>>
    {
        public async Task<ErrorOr<ExampleDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var example = await context.Set<Example>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(request.Id);
            }

            // 1. Process Image (Convert to WebP)
            var processResult = await imageService.ProcessAsync(
                request.ImageStream,
                request.ImageName,
                maxWidth: 640,
                generateThumbnail: false,
                generateResponsive: false,
                ct: cancellationToken);

            if (processResult.IsError)
            {
                return processResult.Errors;
            }

            // 2. Save New File
            var processed = processResult.Value;
            var saveResult = await fileService.SaveFileAsync(
                processed.Main.Stream,
                processed.Main.FileName,
                new FileUploadOptions("examples"),
                cancellationToken);

            if (saveResult.IsError)
            {
                return saveResult.Errors;
            }

            // 3. Remove Old File if exists
            if (!string.IsNullOrEmpty(example.ImageUrl))
            {
                var oldFileId = example.ImageUrl.Replace("/api/files/", "");
                if (!string.IsNullOrEmpty(oldFileId))
                {
                    await fileService.DeleteFileAsync(oldFileId, cancellationToken);
                }
            }

            // 4. Update Example
            var fileName = saveResult.Value.FileId;
            var subdir = saveResult.Value.Subdirectory;
            example.ImageUrl = $"/api/files/{subdir}/{fileName}";

            var embedding = await mlService.GetEmbeddingAsync(example.ImageUrl, example.Id.ToString(), cancellationToken);
            if (embedding != null)
            {
                if (example.Embedding == null)
                {
                    example.Embedding = new ExampleEmbedding
                    {
                        ExampleId = example.Id,
                        Embedding = new Pgvector.Vector(embedding)
                    };
                }
                else
                {
                    example.Embedding.Embedding = new Pgvector.Vector(embedding);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            return new ExampleDetail
            {
                Id = example.Id,
                Name = example.Name,
                Description = example.Description,
                Price = example.Price,
                ImageUrl = example.ImageUrl,
                CreatedAt = example.CreatedAt
            };
        }
    }
}