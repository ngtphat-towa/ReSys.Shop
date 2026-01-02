using Carter;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReSys.Core.Features.Products.CreateProduct;
using ReSys.Core.Features.Products.GetProducts;
using ReSys.Core.Features.Products.GetSimilarProducts;
using ReSys.Core.Features.Products.GetProductById;
using ReSys.Core.Features.Products.UpdateProduct;
using ReSys.Core.Features.Products.DeleteProduct;

using ReSys.Core.Features.Products.UpdateProductImage;
using ErrorOr;

namespace ReSys.Api.Features.Products;

public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        // [AsParameters] binds automatically after our normalization middleware runs.
        group.MapGet("/", async ([AsParameters] GetProducts.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetProducts.Query(request));
            return Results.Ok(result);
        })
        .WithName("GetProducts");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductById.Query(new GetProductById.Request(id)));

            return result.Match(
                product => Results.Ok(product),
                errors => Results.NotFound());
        })
        .WithName("GetProductById");

        group.MapPost("/", async (
            [FromBody] CreateProduct.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateProduct.Command(request), ct);

            return result.Match(
                product => Results.Created($"/api/products/{product.Id}", product),
                errors => errors.Any(e => e.Type == ErrorType.Conflict) 
                    ? Results.Conflict(errors) 
                    : Results.BadRequest(errors));
        })
        .WithName("CreateProduct");

        group.MapPut("/{id}", async (
            Guid id,
            [FromBody] UpdateProduct.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateProduct.Command(id, request), ct);

            return result.Match(
                product => Results.Ok(product),
                errors => errors.Any(e => e.Type == ErrorType.Conflict) 
                    ? Results.Conflict(errors) 
                    : errors.Any(e => e.Type == ErrorType.NotFound)
                        ? Results.NotFound(errors)
                        : Results.BadRequest(errors));
        })
        .WithName("UpdateProduct");

        group.MapPost("/{id}/image", async (
            Guid id,
            IFormFile image,
            ISender sender,
            CancellationToken ct) =>
        {
            using var stream = image.OpenReadStream();

            var request = new UpdateProductImage.Request(id, stream, image.FileName);
            var result = await sender.Send(new UpdateProductImage.Command(request), ct);

            return result.Match(
                product => Results.Ok(product),
                errors => Results.NotFound());
        })
        .WithName("UpdateProductImage")
        .DisableAntiforgery();

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProduct.Command(id));

            return result.Match(
                _ => Results.NoContent(),
                errors => Results.NotFound());
        })
                .WithName("DeleteProduct");
        
        
                group.MapGet("/{id}/similar", async (Guid id, ISender sender) =>
                {
                    var result = await sender.Send(new GetSimilarProducts.Query(new GetSimilarProducts.Request(id)));
        
                    return result.Match(
                        products => Results.Ok(products),
                        errors => Results.NotFound());
                })
                .WithName("GetSimilarProducts");
            }
        }