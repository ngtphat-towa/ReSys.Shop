using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Admin.Settings.ShippingMethods;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Admin.Settings;

public class ShippingMethodEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/settings/shipping-methods")
            .WithTags("Admin.Settings.ShippingMethods")
            .RequireAuthorization();

        group.MapGet("", async (ISender sender) =>
        {
            var result = await sender.Send(new GetShippingMethods.Query());
            return result.ToTypedApiResponse();
        });

        group.MapGet("{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetShippingMethodById.Query(id));
            return result.ToTypedApiResponse();
        });

        group.MapPost("", async (CreateShippingMethod.Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToTypedApiResponse("Shipping method created");
        });

        group.MapPut("{id}", async (Guid id, [FromBody] UpdateShippingMethod.Command command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            var result = await sender.Send(command);
            return result.ToTypedApiResponse("Shipping method updated");
        });

        group.MapDelete("{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteShippingMethod.Command(id));
            return result.ToTypedApiResponse("Shipping method deleted");
        });

        group.MapPatch("{id}/status", async (Guid id, [FromBody] bool isActive, ISender sender) =>
        {
            var result = await sender.Send(new UpdateShippingMethodStatus.Command(id, isActive));
            return result.ToTypedApiResponse("Status updated");
        });
    }
}
