using ErrorOr;

namespace ReSys.Core.Domain.Identity.Tokens;

public static class RefreshTokenErrors
{
    public static Error NotFound => Error.NotFound(
        code: "RefreshToken.NotFound",
        description: "Refresh token not found.");

    public static Error Expired => Error.Validation(
        code: "RefreshToken.Expired",
        description: "Refresh token has expired.");

    public static Error Revoked => Error.Validation(
        code: "RefreshToken.Revoked",
        description: "Refresh token has been revoked.");

    public static Error GenerationFailed => Error.Failure(
        code: "RefreshToken.GenerationFailed",
        description: "Failed to generate refresh token.");

    public static Error RotationFailed => Error.Failure(
        code: "RefreshToken.RotationFailed",
        description: "Failed to rotate refresh token.");
}
