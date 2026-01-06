using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Identity.Contracts;
using ReSys.Core.Features.Identity.Users.AssignRoles;
using ReSys.Core.Features.Identity.Users.CreateUser;
using ReSys.Core.Features.Identity.Users.DeleteUser;
using ReSys.Core.Features.Identity.Users.GetUserById;
using ReSys.Core.Features.Identity.Users.GetUsers;
using ReSys.Core.Features.Identity.Users.LockUser;
using ReSys.Core.Features.Identity.Users.UnlockUser;
using ReSys.Core.Features.Identity.Users.UpdateUser;

namespace ReSys.Api.Features.Account;

public class UsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
                       .WithTags("Users")
                       .RequireAuthorization(); // TODO: Add "Permissions.Users.Manage" policy later

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetUsers.Query());
            return result.ToApiResponse();
        });

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetUserById.Query(id));
            return result.ToApiResponse();
        });

        group.MapPost("/", async ([FromBody] CreateUserRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateUser.Command(request));
            return result.ToApiCreatedResponse(id => $"/api/users/{id}");
        });

        group.MapPut("/{id}", async (string id, [FromBody] UpdateUserRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateUser.Command(id, request));
            return result.ToApiResponse();
        });

        group.MapDelete("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUser.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        });

        group.MapPost("/{id}/roles", async (string id, [FromBody] AssignRolesRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AssignRoles.Command(id, request));
            return result.ToApiResponse();
        });

        group.MapPost("/{id}/lock", async (string id, [FromBody] LockUserRequest request, ISender sender) =>
        {
            var result = await sender.Send(new LockUser.Command(id, request));
            return result.ToApiResponse();
        });

        group.MapPost("/{id}/unlock", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new UnlockUser.Command(id));
            return result.ToApiResponse();
        });
    }
}

