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
using ReSys.Core.Common.Models;
using ReSys.Api.Infrastructure.Extensions;

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
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProducts");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductById.Query(new GetProductById.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetProductById");

        group.MapPost("/", async (
            [FromBody] CreateProduct.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateProduct.Command(request), ct);
            return result.ToApiCreatedResponse(product => $"/api/products/{product.Id}");
        })
        .WithName("CreateProduct");

        group.MapPut("/{id}", async (
            Guid id,
            [FromBody] UpdateProduct.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateProduct.Command(id, request), ct);
            return result.ToApiResponse();
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
            return result.ToApiResponse();
        })
        .WithName("UpdateProductImage")
        .DisableAntiforgery();

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProduct.Command(id));

            if (result.IsError)
            {
                return result.ToApiResponse();
            }

            return Results.NoContent();
        })
        .WithName("DeleteProduct");
        
        group.MapGet("/{id}/similar", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetSimilarProducts.Query(new GetSimilarProducts.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetSimilarProducts");
    }
}