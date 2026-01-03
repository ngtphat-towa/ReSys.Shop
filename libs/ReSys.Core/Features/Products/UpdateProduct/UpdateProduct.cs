using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.UpdateProduct;

public static class UpdateProduct
{
    public record Request : ProductInput;

    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<ProductDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
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
            var product = await _context.Set<Product>().FindAsync(new object[] { command.Id }, cancellationToken);

            if (product is null)
            {
                return ProductErrors.NotFound(command.Id);
            }

            var request = command.Request;

            if (await _context.Set<Product>().AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken))
            {
                return ProductErrors.DuplicateName;
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.ImageUrl = request.ImageUrl;

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
