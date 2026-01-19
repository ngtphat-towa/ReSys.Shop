using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReSys.Core.Features.Identity.Internal.Login;
using ReSys.Core.Features.Identity.Internal.Register;
using ReSys.Core.Features.Identity.Internal.Confirmations;
using ReSys.Core.Features.Identity.Internal.Password;
using ReSys.Core.Features.Identity.Internal.Sessions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Identity.Internal;

public sealed class InternalIdentityModule : ICarterModule
{
    private const string BaseRoute = "api/auth";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - Internal")
            .DisableAntiforgery();

        // 1. Authentication
        group.MapPost("login", LoginHandler);
        group.MapPost("register", RegisterHandler);
        
        // 2. Sessions
        var sessionGroup = group.MapGroup("session");
        sessionGroup.MapGet("/", GetSessionHandler).RequireAuthorization();
        sessionGroup.MapPost("refresh", RefreshHandler);
        sessionGroup.MapPost("logout", LogOutHandler);

        // 3. Confirmations
        group.MapPost("confirm-email", ConfirmEmailHandler);
        group.MapPost("confirm-phone", ConfirmPhoneHandler);
        group.MapPost("resend-email", ResendEmailHandler);
        group.MapPost("resend-phone", ResendPhoneHandler);
        
        // 4. Password Management
        var passwordGroup = group.MapGroup("password");
        passwordGroup.MapPost("forgot", ForgotPasswordHandler);
        passwordGroup.MapPost("reset", ResetPasswordHandler);
        passwordGroup.MapPost("change", ChangePasswordHandler).RequireAuthorization();

        // 5. Development
        if (app.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            group.MapPost("login/dev", DevLoginHandler);
        }
    }

    private static async Task<IResult> LoginHandler([FromBody] Login.Request request, ISender mediator, HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var result = await mediator.Send(new Login.Command(request with { IpAddress = ip }));
        return result.ToTypedApiResponse("Logged in successfully");
    }

    private static async Task<IResult> RegisterHandler([FromBody] Register.Request request, ISender mediator)
    {
        var result = await mediator.Send(new Register.Command(request));
        return result.ToTypedApiResponse("Registered successfully");
    }

    private static async Task<IResult> RefreshHandler([FromBody] Refresh.Request request, ISender mediator, HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var result = await mediator.Send(new Refresh.Command(request with { IpAddress = ip }));
        return result.ToTypedApiResponse("Token refreshed successfully");
    }

    private static async Task<IResult> GetSessionHandler(ISender mediator)
    {
        var result = await mediator.Send(new GetSession.Query());
        return result.ToTypedApiResponse("Session retrieved successfully");
    }

    private static async Task<IResult> LogOutHandler([FromBody] LogOut.Request request, ISender mediator, HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var result = await mediator.Send(new LogOut.Command(request with { IpAddress = ip }));
        return result.ToTypedApiResponse("Logged out successfully");
    }

    private static async Task<IResult> ConfirmEmailHandler([FromBody] ConfirmEmail.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ConfirmEmail.Command(request));
        return result.ToTypedApiResponse("Email confirmed successfully");
    }

    private static async Task<IResult> ConfirmPhoneHandler([FromBody] ConfirmPhone.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ConfirmPhone.Command(request));
        return result.ToTypedApiResponse("Phone confirmed successfully");
    }

    private static async Task<IResult> ResendEmailHandler([FromBody] ResendEmailConfirmation.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ResendEmailConfirmation.Command(request));
        return result.ToTypedApiResponse("Verification email sent");
    }

    private static async Task<IResult> ResendPhoneHandler([FromBody] ResendPhoneVerification.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ResendPhoneVerification.Command(request));
        return result.ToTypedApiResponse("Verification SMS sent");
    }

    private static async Task<IResult> ForgotPasswordHandler([FromBody] ForgotPassword.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ForgotPassword.Command(request));
        return result.ToTypedApiResponse("Reset link sent if account exists");
    }

    private static async Task<IResult> ResetPasswordHandler([FromBody] ResetPassword.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ResetPassword.Command(request));
        return result.ToTypedApiResponse("Password reset successfully");
    }

    private static async Task<IResult> ChangePasswordHandler([FromBody] ChangePassword.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ChangePassword.Command(request));
        return result.ToTypedApiResponse("Password changed successfully");
    }

    private static async Task<IResult> DevLoginHandler(ISender mediator)
    {
        var request = new Login.Request("admin@resys.shop", "Seeder@123", true);
        var result = await mediator.Send(new Login.Command(request));
        return result.ToTypedApiResponse("Development auto login");
    }
}