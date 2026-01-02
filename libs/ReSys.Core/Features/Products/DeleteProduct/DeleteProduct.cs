using ErrorOr;
using MediatR;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.DeleteProduct;

public static class DeleteProduct
{
    public class Command : IRequest<ErrorOr<Deleted>>
    {
        public Guid Id { get; set; }

        public Command(Guid id) => Id = id;
    }

    public class Handler : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await _context.Set<Product>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (product is null)
            {
                return ProductErrors.NotFound(request.Id);
            }

            _context.Set<Product>().Remove(product);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}