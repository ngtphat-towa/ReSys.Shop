using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Identity.Roles.CreateRole;
using ReSys.Core.Features.Identity.Roles.DeleteRole;
using ReSys.Core.Features.Identity.Roles.GetRoleById;
using ReSys.Core.Features.Identity.Roles.GetRoles;
using ReSys.Core.Features.Identity.Roles.UpdateRole;
using ReSys.Core.Features.Identity.Roles.UpdateRolePermissions;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Api.Features.Account;

public class RolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
                       .WithTags("Roles")
                       .RequireAuthorization();

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetRoles.Query());
            return result.ToApiResponse();
        });

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetRoleById.Query(id));
            return result.ToApiResponse();
        });

        group.MapPost("/", async ([FromBody] CreateRoleRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateRole.Command(request));
            return result.ToApiCreatedResponse(id => $"/api/roles/{id}");
        });

        group.MapPut("/{id}", async (string id, [FromBody] UpdateRoleRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRole.Command(id, request));
            return result.ToApiResponse();
        });

        group.MapDelete("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRole.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        });

        group.MapPut("/{id}/permissions", async (string id, [FromBody] UpdateRolePermissionsRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRolePermissions.Command(id, request));
            return result.ToApiResponse();
        });
    }
}

