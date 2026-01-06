namespace ReSys.Core.Features.Identity.Contracts;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
