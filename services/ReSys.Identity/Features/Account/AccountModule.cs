using System.Security.Claims;


using Carter;


using MediatR;


using Microsoft.AspNetCore.Mvc;


using ReSys.Identity.Presentation.Extensions;

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

            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .WithName("RegisterUser");

        group.MapPost("/login", async ([FromBody] Login.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new Login.Command(request);
            var result = await sender.Send(command, ct);

            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .WithName("Login");

        group.MapPost("/logout", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Logout.Command(), ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequireAuthorization()
        .WithName("Logout");

        group.MapPost("/change-email", async ([FromBody] ChangeEmail.Request request, ISender sender, HttpContext httpContext, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var command = new ChangeEmail.Command(userId, request);
            var result = await sender.Send(command, ct);

            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequireAuthorization()
        .WithName("ChangeEmail");

        group.MapPost("/confirm-email-change", async ([FromBody] ConfirmEmailChange.Request request, ISender sender, HttpContext httpContext, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var command = new ConfirmEmailChange.Command(userId, request);
            var result = await sender.Send(command, ct);

            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequireAuthorization()
        .WithName("ConfirmEmailChange");

        group.MapPost("/change-password", async ([FromBody] ChangePassword.Request request, ISender sender, HttpContext httpContext, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var command = new ChangePassword.Command(userId, request);
            var result = await sender.Send(command, ct);

            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequireAuthorization()
        .WithName("ChangePassword");
    }
}