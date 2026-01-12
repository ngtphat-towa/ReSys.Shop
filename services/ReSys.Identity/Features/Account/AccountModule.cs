using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using static ReSys.Identity.Features.Account.Register;

namespace ReSys.Identity.Features.Account;

public class AccountModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/account")
            .WithTags("Account");

        group.MapPost("/register", async ([FromBody] Register.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new Register.Command(request);
            var result = await sender.Send(command, ct);

            return result.IsError ? Results.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description })) : Results.Ok();
        })
        .WithName("RegisterUser");
    }
}