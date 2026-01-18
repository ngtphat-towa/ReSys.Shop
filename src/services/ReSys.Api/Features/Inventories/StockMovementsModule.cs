using Carter;

using MediatR;

using ReSys.Core.Features.Inventories.Movements.GetStockMovementsPagedList;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Inventories;

public class StockMovementsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/inventories/movements")
            .WithTags("Stock Movements");

        group.MapGet("/", async ([AsParameters] GetStockMovementsPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetStockMovementsPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetStockMovements");
    }
}
