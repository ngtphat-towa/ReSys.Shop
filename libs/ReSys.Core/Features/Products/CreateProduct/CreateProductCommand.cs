using ErrorOr;
using MediatR;
using ReSys.Core.Entities;

namespace ReSys.Core.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price) : IRequest<ErrorOr<Product>>;
