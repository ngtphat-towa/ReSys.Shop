using Carter;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Inventories.Stocks.AdjustStock;
using ReSys.Core.Features.Inventories.Stocks.DeleteStockItem;
using ReSys.Core.Features.Inventories.Stocks.RestoreStockItem;
using ReSys.Core.Features.Inventories.Stocks.GetStockItemDetail;
using ReSys.Core.Features.Inventories.Stocks.GetStockItemsPagedList;
using ReSys.Core.Features.Inventories.Stocks.UpdateBackorderPolicy;
using ReSys.Core.Features.Inventories.Stocks.Common;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Inventories;

public class StockItemsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/inventories/stocks")
            .WithTags("Stock Items");

        group.MapGet("/", async ([AsParameters] GetStockItemsPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetStockItemsPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetStockItems");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetStockItemDetail.Query(new GetStockItemDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetStockItemById");

        group.MapPost("/{id:guid}/adjust", async (Guid id, [FromBody] StockAdjustmentRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdjustStock.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("AdjustStock");

        group.MapPut("/{id:guid}/backorder-policy", async (Guid id, [FromBody] BackorderPolicyRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateBackorderPolicy.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateBackorderPolicy");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteStockItem.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteStockItem");

        group.MapPost("/{id:guid}/restore", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RestoreStockItem.Command(id));
            return result.ToApiResponse();
        })
        .WithName("RestoreStockItem");
    }
}