using ErrorOr;
using MediatR;
using ReSys.Core.Entities;

namespace ReSys.Core.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    Stream? ImageStream,
    string? ImageName) : IRequest<ErrorOr<Product>>;
