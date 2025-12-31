using ErrorOr;
using MediatR;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Stream? ImageStream,
    string? ImageName) : IRequest<ErrorOr<Product>>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ErrorOr<Product>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public UpdateProductCommandHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<ErrorOr<Product>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "The product with the specified ID was not found.");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;

        if (request.ImageStream is not null && request.ImageName is not null)
        {
            var fileName = await _fileService.SaveFileAsync(request.ImageStream, request.ImageName, cancellationToken);
            product.ImageUrl = $"/api/files/{fileName}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }
}
