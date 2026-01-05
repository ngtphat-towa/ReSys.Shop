namespace ReSys.Identity.Features.Account.Contracts;

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record ConfirmEmailRequest(string UserId, string Token);
public record LockUserRequest(DateTimeOffset? LockoutEnd);
public record UpdateRoleRequest(string Name);
