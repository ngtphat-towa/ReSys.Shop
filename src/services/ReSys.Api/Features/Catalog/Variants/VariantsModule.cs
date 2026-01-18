using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Catalog.Products.Variants.CreateVariant;
using ReSys.Core.Features.Catalog.Products.Variants.DeleteVariant;
using ReSys.Core.Features.Catalog.Products.Variants.GetVariantDetail;
using ReSys.Core.Features.Catalog.Products.Variants.GetVariantSelectList;
using ReSys.Core.Features.Catalog.Products.Variants.GetVariantsPagedList;
using ReSys.Core.Features.Catalog.Products.Variants.ManageVariantOptionValues;
using ReSys.Core.Features.Catalog.Products.Variants.SetMasterVariant;
using ReSys.Core.Features.Catalog.Products.Variants.SetVariantPrice;
using ReSys.Core.Features.Catalog.Products.Variants.UpdateVariant;
using ReSys.Core.Features.Catalog.Products.Variants.DiscontinueVariant;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Catalog.Variants;

public class VariantsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/catalog/variants")
            .WithTags("Variants");

        group.MapGet("/", async ([AsParameters] GetVariantsPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetVariantsPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetVariants");

        group.MapGet("/select-list", async ([AsParameters] GetVariantSelectList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetVariantSelectList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetVariantSelectList");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetVariantDetail.Query(new GetVariantDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetVariantById");

        group.MapPost("/", async ([FromBody] CreateVariant.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateVariant.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/catalog/variants/{x.Id}");
        })
        .WithName("CreateVariant");

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateVariant.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateVariant.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateVariant");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteVariant.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteVariant");

        group.MapPatch("/{id:guid}/discontinue", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DiscontinueVariant.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("DiscontinueVariant");

        group.MapPost("/{id:guid}/set-master", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SetMasterVariant.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("SetMasterVariant");

        group.MapPost("/{id:guid}/price", async (Guid id, [FromBody] SetVariantPrice.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SetVariantPrice.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("SetVariantPrice");

        group.MapPut("/{id:guid}/option-values", async (Guid id, [FromBody] ManageVariantOptionValues.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ManageVariantOptionValues.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("ManageVariantOptionValues");
    }
}