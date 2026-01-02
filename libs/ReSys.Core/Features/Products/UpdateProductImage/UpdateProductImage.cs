using ErrorOr;
using MediatR;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.UpdateProductImage;

public static class UpdateProductImage
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

    public record Command(Request Request) : IRequest<ErrorOr<ProductDetail>>;

    public class Handler : IRequestHandler<Command, ErrorOr<ProductDetail>>
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

        public async Task<ErrorOr<ProductDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var product = await _context.Set<Product>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (product is null)
            {
                return ProductErrors.NotFound(request.Id);
            }

            var fileName = await _fileService.SaveFileAsync(request.ImageStream, request.ImageName, cancellationToken);
            product.ImageUrl = $"/api/files/{fileName}";

            var embedding = await _mlService.GetEmbeddingAsync(product.ImageUrl, product.Id.ToString(), cancellationToken);
            if (embedding != null)
            {
                if (product.Embedding == null)
                {
                    product.Embedding = new ProductEmbedding
                    {
                        ProductId = product.Id,
                        Embedding = new Pgvector.Vector(embedding)
                    };
                }
                else
                {
                    product.Embedding.Embedding = new Pgvector.Vector(embedding);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ProductDetail
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt
            };
        }
    }
}