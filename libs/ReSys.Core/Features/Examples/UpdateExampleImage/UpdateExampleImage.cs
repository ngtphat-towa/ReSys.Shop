using ReSys.Core.Features.Examples.Common;

namespace ReSys.Core.Features.Examples.UpdateExampleImage;

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

        public Handler(
            IApplicationDbContext context,
            IFileService fileService,
            IMlService mlService)
        {
            _context = context;
            _fileService = fileService;
            _mlService = mlService;
        }

        public async Task<ErrorOr<ExampleDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var example = await _context.Set<Example>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(request.Id);
            }

            var fileName = await _fileService.SaveFileAsync(request.ImageStream, request.ImageName, cancellationToken);
            example.ImageUrl = $"/api/files/{fileName}";

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