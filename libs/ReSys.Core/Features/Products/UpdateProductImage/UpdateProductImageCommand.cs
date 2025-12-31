using ErrorOr;
using MediatR;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.UpdateProductImage;

public record UpdateProductImageCommand(
    Guid Id,
    Stream ImageStream,
    string ImageName) : IRequest<ErrorOr<Product>>;

public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand, ErrorOr<Product>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMlService _mlService;

    public UpdateProductImageCommandHandler(
        IApplicationDbContext context, 
        IFileService fileService,
        IMlService mlService)
    {
        _context = context;
        _fileService = fileService;
        _mlService = mlService;
    }

    public async Task<ErrorOr<Product>> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "The product with the specified ID was not found.");
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

        return product;
    }
}
