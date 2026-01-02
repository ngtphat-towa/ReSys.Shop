using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.GetProductById;

public static class GetProductById
{
    public class Request(Guid id)
    {
        public Guid Id { get; set; } = id;
    }

    public record Query(Request Request) : IRequest<ErrorOr<ProductDetail>>;

    public class Handler : IRequestHandler<Query, ErrorOr<ProductDetail>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<ProductDetail>> Handle(Query query, CancellationToken cancellationToken)
        {
            var response = await _context.Set<Product>()
                .AsNoTracking()
                .Where(x => x.Id == query.Request.Id)
                .Select(ProductDetail.Projection)
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return ProductErrors.NotFound(query.Request.Id);
            }

            return response;
        }
    }
}