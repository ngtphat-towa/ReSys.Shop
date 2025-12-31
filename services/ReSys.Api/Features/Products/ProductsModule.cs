using Carter;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReSys.Api.Features.Products.CreateProduct;
using ReSys.Api.Features.Products.GetProducts;
using ReSys.Api.Features.Products.GetSimilarProducts;

namespace ReSys.Api.Features.Products;

public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery());
            return Results.Ok(result);
        })
        .WithName("GetProducts");

        group.MapPost("/", async (
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal price,
            IFormFile? image,
            ISender sender,
            CancellationToken ct) =>
        {
            Stream? stream = image?.OpenReadStream();
            
            var command = new CreateProductCommand(
                name, description, price, stream, image?.FileName);

            var result = await sender.Send(command, ct);

            return result.Match(
                product => Results.Created($"/api/products/{product.Id}", product),
                errors => Results.BadRequest(errors));
        })
        .WithName("CreateProduct")
        .DisableAntiforgery();


        group.MapGet("/{id}/similar", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetSimilarProductsQuery(id));

            return result.Match(
                products => Results.Ok(products),
                errors => Results.NotFound());
        })
        .WithName("GetSimilarProducts");
    }
}