using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.CreateProduct;

public static class CreateProduct
{
    public record Request : ProductInput;

    public record Command(Request Request) : IRequest<ErrorOr<ProductDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }

        private class RequestValidator : ProductValidator<Request> { }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<ProductDetail>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<ProductDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (await _context.Set<Product>().AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return ProductErrors.DuplicateName;
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<Product>().Add(product);
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