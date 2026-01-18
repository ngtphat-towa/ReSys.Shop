using Carter;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Inventories.Transfers.CreateStockTransfer;
using ReSys.Core.Features.Inventories.Transfers.AddTransferItem;
using ReSys.Core.Features.Inventories.Transfers.ShipStockTransfer;
using ReSys.Core.Features.Inventories.Transfers.ReceiveStockTransfer;
using ReSys.Core.Features.Inventories.Transfers.CancelStockTransfer;
using ReSys.Core.Features.Inventories.Transfers.RemoveStockTransferItem;
using ReSys.Core.Features.Inventories.Transfers.GetStockTransferDetail;
using ReSys.Core.Features.Inventories.Transfers.GetStockTransfersPagedList;
using ReSys.Core.Features.Inventories.Transfers.Common;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Inventories;

public class StockTransfersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/inventories/transfers")
            .WithTags("Stock Transfers");

        group.MapGet("/", async ([AsParameters] GetStockTransfersPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetStockTransfersPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetStockTransfers");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetStockTransferDetail.Query(new GetStockTransferDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetStockTransferById");

        group.MapPost("/", async ([FromBody] CreateStockTransfer.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateStockTransfer.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/inventories/transfers/{x.Id}");
        })
        .WithName("CreateStockTransfer");

        group.MapPost("/{id:guid}/items", async (Guid id, [FromBody] AddTransferItemRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AddTransferItem.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("AddStockTransferItem");

        group.MapDelete("/{id:guid}/items/{variantId:guid}", async (Guid id, Guid variantId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RemoveStockTransferItem.Command(id, variantId), ct);
            return result.ToApiResponse();
        })
        .WithName("RemoveStockTransferItem");

        group.MapPost("/{id:guid}/ship", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ShipStockTransfer.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("ShipStockTransfer");

        group.MapPost("/{id:guid}/receive", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ReceiveStockTransfer.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("ReceiveStockTransfer");

        group.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelStockTransfer.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("CancelStockTransfer");
    }
}