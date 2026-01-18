using Carter;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Inventories.Locations.CreateStockLocation;
using ReSys.Core.Features.Inventories.Locations.UpdateStockLocation;
using ReSys.Core.Features.Inventories.Locations.DeleteStockLocation;
using ReSys.Core.Features.Inventories.Locations.RestoreStockLocation;
using ReSys.Core.Features.Inventories.Locations.GetStockLocationDetail;
using ReSys.Core.Features.Inventories.Locations.GetStockLocationsPagedList;
using ReSys.Core.Features.Inventories.Locations.ToggleStockLocationStatus;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Inventories;

public class StockLocationsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/inventories/locations")
            .WithTags("Stock Locations");

        group.MapGet("/", async ([AsParameters] GetStockLocationsPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetStockLocationsPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetStockLocations");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetStockLocationDetail.Query(new GetStockLocationDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetStockLocationById");

        group.MapPost("/", async ([FromBody] CreateStockLocation.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateStockLocation.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/inventories/locations/{x.Id}");
        })
        .WithName("CreateStockLocation");

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateStockLocation.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateStockLocation.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateStockLocation");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteStockLocation.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteStockLocation");

        group.MapPost("/{id:guid}/restore", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RestoreStockLocation.Command(id));
            return result.ToApiResponse();
        })
        .WithName("RestoreStockLocation");

        group.MapPatch("/{id:guid}/toggle-status", async (Guid id, [FromQuery] bool activate, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ToggleStockLocationStatus.Command(id, activate), ct);
            return result.ToApiResponse();
        })
        .WithName("ToggleStockLocationStatus");
    }
}