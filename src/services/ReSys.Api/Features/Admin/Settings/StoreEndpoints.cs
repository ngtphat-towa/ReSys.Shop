using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Admin.Settings.Stores;
using ReSys.Core.Features.Admin.Settings.Stores.Common;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Admin.Settings;

public class StoreEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/settings/stores")
            .WithTags("Admin.Settings.Stores")
            .RequireAuthorization();

        group.MapGet("", async (ISender sender) =>
        {
            var result = await sender.Send(new GetStores.Query());
            return result.ToTypedApiResponse();
        });

        group.MapGet("{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetStoreById.Query(id));
            return result.ToTypedApiResponse();
        });

        group.MapPost("", async (StoreInput request, ISender sender) =>
        {
            var result = await sender.Send(new CreateStore.Command(request));
            return result.ToTypedApiResponse("Store created");
        });

        group.MapPut("{id}", async (Guid id, StoreInput request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateStore.Command(id, request));
            return result.ToTypedApiResponse("Store updated");
        });
    }
}
