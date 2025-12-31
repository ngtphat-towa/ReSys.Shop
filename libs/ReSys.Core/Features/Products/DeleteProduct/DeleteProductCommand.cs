using ErrorOr;
using MediatR;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "The product with the specified ID was not found.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
