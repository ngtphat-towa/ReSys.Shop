using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Catalog.Products.CreateProduct;
using ReSys.Core.Features.Catalog.Products.DeleteProduct;
using ReSys.Core.Features.Catalog.Products.GetProductDetail;
using ReSys.Core.Features.Catalog.Products.GetProductsPagedList;
using ReSys.Core.Features.Catalog.Products.GetProductSelectList;
using ReSys.Core.Features.Catalog.Products.UpdateProduct;
using ReSys.Core.Features.Catalog.Products.ActivateProduct;
using ReSys.Core.Features.Catalog.Products.ArchiveProduct;
using ReSys.Core.Features.Catalog.Products.DraftProduct;
using ReSys.Core.Features.Catalog.Products.DiscontinueProduct;
using ReSys.Core.Features.Catalog.Products.GetProductBySlug;
using ReSys.Core.Features.Catalog.Products.Classifications.GetProductClassifications;
using ReSys.Core.Features.Catalog.Products.Classifications.ManageProductClassifications;
using ReSys.Core.Features.Catalog.Products.OptionTypes.GetProductOptionTypes;
using ReSys.Core.Features.Catalog.Products.OptionTypes.ManageProductOptionTypes;
using ReSys.Core.Features.Catalog.Products.Properties.GetProductProperties;
using ReSys.Core.Features.Catalog.Products.Properties.ManageProductProperties;
using ReSys.Core.Features.Catalog.Products.Images.GetProductImages;
using ReSys.Core.Features.Catalog.Products.Images.UploadProductImage;
using ReSys.Core.Features.Catalog.Products.Images.UpdateProductImage;
using ReSys.Core.Features.Catalog.Products.Images.RemoveProductImage;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Catalog.Products;

public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/catalog/products")
            .WithTags("Products");

        group.MapGet("/", async ([AsParameters] GetProductsPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetProductsPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProducts");

        group.MapGet("/select-list", async ([AsParameters] GetProductSelectList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetProductSelectList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProductSelectList");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductDetail.Query(new GetProductDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetProductById");

        group.MapGet("/slug/{slug}", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new GetProductBySlug.Query(new GetProductBySlug.Request(slug)));
            return result.ToApiResponse();
        })
        .WithName("GetProductBySlug");

        group.MapPost("/", async ([FromBody] CreateProduct.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateProduct.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/catalog/products/{x.Id}");
        })
        .WithName("CreateProduct");

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateProduct.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateProduct.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateProduct");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProduct.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteProduct");

        // Status Management
        group.MapPatch("/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ActivateProduct.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("ActivateProduct");

        group.MapPatch("/{id:guid}/archive", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ArchiveProduct.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("ArchiveProduct");

        group.MapPatch("/{id:guid}/draft", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DraftProduct.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("DraftProduct");

        group.MapPatch("/{id:guid}/discontinue", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DiscontinueProduct.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("DiscontinueProduct");

        // Classifications
        group.MapGet("/{id:guid}/classifications", async (Guid id, [AsParameters] GetProductClassifications.Request request, ISender sender) =>
        {
            request.ProductId = id;
            var result = await sender.Send(new GetProductClassifications.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProductClassifications");

        group.MapPut("/{id:guid}/classifications", async (Guid id, [FromBody] ManageProductClassifications.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ManageProductClassifications.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("ManageProductClassifications");

        // Option Types
        group.MapGet("/{id:guid}/option-types", async (Guid id, [AsParameters] GetProductOptionTypes.Request request, ISender sender) =>
        {
            request.ProductId = id;
            var result = await sender.Send(new GetProductOptionTypes.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProductOptionTypes");

        group.MapPut("/{id:guid}/option-types", async (Guid id, [FromBody] ManageProductOptionTypes.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ManageProductOptionTypes.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("ManageProductOptionTypes");

        // Properties
        group.MapGet("/{id:guid}/properties", async (Guid id, [AsParameters] GetProductProperties.Request request, ISender sender) =>
        {
            request.ProductId = id;
            var result = await sender.Send(new GetProductProperties.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProductProperties");

        group.MapPut("/{id:guid}/properties", async (Guid id, [FromBody] ManageProductProperties.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ManageProductProperties.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("ManageProductProperties");

        // Images
        group.MapGet("/{id:guid}/images", async (Guid id, [AsParameters] GetProductImages.Request request, ISender sender) =>
        {
            request.ProductId = id;
            var result = await sender.Send(new GetProductImages.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetProductImages");

        group.MapPost("/{id:guid}/images", async (
            Guid id,
            IFormFile file,
            [FromQuery] Guid? variantId,
            [FromQuery] ReSys.Core.Domain.Catalog.Products.Images.ProductImage.ProductImageType role,
            [FromQuery] string? alt,
            ISender sender,
            CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var request = new UploadProductImage.Request(id, stream, file.FileName, variantId, role, alt);
            var result = await sender.Send(new UploadProductImage.Command(request), ct);
            return result.ToApiResponse();
        })
        .WithName("UploadProductImage")
        .DisableAntiforgery();

        group.MapPut("/{id:guid}/images/{imageId:guid}", async (
            Guid id,
            Guid imageId,
            [FromBody] UpdateProductImage.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            request.ProductId = id;
            var result = await sender.Send(new UpdateProductImage.Command(imageId, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateProductImage");

        group.MapDelete("/{id:guid}/images/{imageId:guid}", async (Guid id, Guid imageId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveProductImage.Command(id, imageId));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("RemoveProductImage");
    }
}
