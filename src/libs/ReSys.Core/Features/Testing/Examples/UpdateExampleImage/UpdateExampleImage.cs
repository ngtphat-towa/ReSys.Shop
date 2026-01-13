using ErrorOr;

using MediatR;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Common.Storage;
using ReSys.Core.Common.Imaging;
using ReSys.Core.Common.Ml;

namespace ReSys.Core.Features.Testing.Examples.UpdateExampleImage;

public static class UpdateExampleImage
{
    public class Request
    {
        public Guid Id { get; set; }
        public Stream ImageStream { get; set; } = null!;
        public string ImageName { get; set; } = string.Empty;

        public Request(Guid id, Stream imageStream, string imageName)
        {
            Id = id;
            ImageStream = imageStream;
            ImageName = imageName;
        }
    }

    public record Command(Request Request) : IRequest<ErrorOr<ExampleDetail>>;

    public class Handler : IRequestHandler<Command, ErrorOr<ExampleDetail>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IMlService _mlService;
        private readonly IImageService _imageService;

        public Handler(
            IApplicationDbContext context,
            IFileService fileService,
            IMlService mlService,
            IImageService imageService)
        {
            _context = context;
            _fileService = fileService;
            _mlService = mlService;
            _imageService = imageService;
        }

        public async Task<ErrorOr<ExampleDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var example = await _context.Set<Example>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(request.Id);
            }

            // 1. Process Image (Convert to WebP)
            var processResult = await _imageService.ProcessAsync(
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
            var saveResult = await _fileService.SaveFileAsync(
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
                    await _fileService.DeleteFileAsync(oldFileId, cancellationToken);
                }
            }

            // 4. Update Example
            var fileName = saveResult.Value.FileId;
            var subdir = saveResult.Value.Subdirectory;
            example.ImageUrl = $"/api/files/{subdir}/{fileName}";

            var embedding = await _mlService.GetEmbeddingAsync(example.ImageUrl, example.Id.ToString(), cancellationToken);
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

            await _context.SaveChangesAsync(cancellationToken);

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