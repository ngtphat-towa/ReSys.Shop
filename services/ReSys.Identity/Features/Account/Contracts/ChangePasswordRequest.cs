namespace ReSys.Identity.Features.Account.Contracts;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
