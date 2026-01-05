using Carter;
using ReSys.Core.Domain.Identity;

namespace ReSys.Identity.Features.Account;

public class PermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/permissions", () => Results.Ok(Permissions.All()))
           .WithTags("Roles");
    }
}
