using Carter;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReSys.Core.Features.Products.CreateProduct;
using ReSys.Core.Features.Products.GetProducts;
using ReSys.Core.Features.Products.GetSimilarProducts;
using ReSys.Core.Features.Products.GetProductById;
using ReSys.Core.Features.Products.UpdateProduct;
using ReSys.Core.Features.Products.DeleteProduct;

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

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));

            return result.Match(
                product => Results.Ok(product),
                errors => Results.NotFound());
        })
        .WithName("GetProductById");

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

        group.MapPut("/{id}", async (
            Guid id,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal price,
            IFormFile? image,
            ISender sender,
            CancellationToken ct) =>
        {
            Stream? stream = image?.OpenReadStream();

            var command = new UpdateProductCommand(
                id, name, description, price, stream, image?.FileName);

            var result = await sender.Send(command, ct);

            return result.Match(
                product => Results.Ok(product),
                errors => Results.NotFound());
        })
        .WithName("UpdateProduct")
        .DisableAntiforgery();

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));

            return result.Match(
                _ => Results.NoContent(),
                errors => Results.NotFound());
        })
        .WithName("DeleteProduct");


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