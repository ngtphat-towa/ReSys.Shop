using Carter;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Inventories.Units.GetInventoryUnitDetail;
using ReSys.Core.Features.Inventories.Units.GetInventoryUnitsPagedList;
using ReSys.Core.Features.Inventories.Units.UpdateInventoryUnitSerialNumber;
using ReSys.Core.Features.Inventories.Units.RestoreInventoryUnit;
using ReSys.Core.Features.Inventories.Units.MarkInventoryUnitDamaged;
using ReSys.Core.Features.Inventories.Units.Common;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Inventories;

public class InventoryUnitsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/inventories/units")
            .WithTags("Inventory Units");

        group.MapGet("/", async ([AsParameters] GetInventoryUnitsPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetInventoryUnitsPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetInventoryUnits");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetInventoryUnitDetail.Query(new GetInventoryUnitDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetInventoryUnitById");

        group.MapPatch("/{id:guid}/serial-number", async (Guid id, [FromBody] UpdateSerialNumberRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateInventoryUnitSerialNumber.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateInventoryUnitSerialNumber");

        group.MapPatch("/{id:guid}/damaged", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new MarkInventoryUnitDamaged.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("MarkInventoryUnitDamaged");

        group.MapPost("/{id:guid}/restore", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RestoreInventoryUnit.Command(id));
            return result.ToApiResponse();
        })
        .WithName("RestoreInventoryUnit");
    }
}
