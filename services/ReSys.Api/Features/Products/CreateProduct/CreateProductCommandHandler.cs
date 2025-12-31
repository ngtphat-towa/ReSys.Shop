using ErrorOr;
using MediatR;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;

namespace ReSys.Api.Features.Products.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ErrorOr<Product>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMlService _mlService;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        IFileService fileService,
        IMlService mlService)
    {
        _context = context;
        _fileService = fileService;
        _mlService = mlService;
    }

    public async Task<ErrorOr<Product>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        if (request.ImageStream != null && request.ImageName != null)
        {
            var imageUrl = await _fileService.SaveFileAsync(request.ImageStream, request.ImageName, cancellationToken);
            product.ImageUrl = imageUrl;

            var embedding = await _mlService.GetEmbeddingAsync(imageUrl, product.Id.ToString(), cancellationToken);
            if (embedding != null)
            {
                product.Embedding = new ProductEmbedding
                {
                    ProductId = product.Id,
                    Embedding = new Pgvector.Vector(embedding)
                };
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }
}
