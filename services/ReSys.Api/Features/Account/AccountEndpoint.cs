using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Identity.Contracts;
using ReSys.Core.Features.Identity.Account.ChangePassword;
using ReSys.Core.Features.Identity.Account.ForgotPassword;
using ReSys.Core.Features.Identity.Account.GetProfile;
using ReSys.Core.Features.Identity.Account.ResetPassword;
using ReSys.Core.Features.Identity.Account.UpdateProfile;

namespace ReSys.Api.Features.Account;

public class AccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/account")
                       .WithTags("Account");

        // Authenticated routes
        group.MapGet("/profile", async (HttpContext context, ISender sender) =>
        {
            var result = await sender.Send(new GetProfile.Query(context.User));
            return result.ToApiResponse();
        }).RequireAuthorization();

        group.MapPut("/profile", async (HttpContext context, [FromBody] UpdateUserRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateProfile.Command(context.User, request));
            return result.ToApiResponse();
        }).RequireAuthorization();

        group.MapPost("/change-password", async (HttpContext context, [FromBody] ChangePasswordRequest request, ISender sender) =>
        {
            var result = await sender.Send(new ChangePassword.Command(context.User, request));
            return result.ToApiResponse();
        }).RequireAuthorization();
        
        // Public routes
        group.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, ISender sender) =>
        {
            var result = await sender.Send(new ForgotPassword.Command(request));
            return result.ToApiResponse();
        });

        group.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, ISender sender) =>
        {
            var result = await sender.Send(new ResetPassword.Command(request));
            return result.ToApiResponse();
        });
    }
}