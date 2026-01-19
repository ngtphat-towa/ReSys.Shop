using ErrorOr;

namespace ReSys.Core.Domain.Identity.Tokens;

public sealed class Jwt
{
    public static class Constraints
    {
        public const int TokenParts = 3;
        public const int MinSecretBytes = 32;
    }

    public static class Errors
    {
        public static Error InvalidFormat => Error.Validation(code: "Jwt.InvalidFormat", description: "Invalid token format");
        public static Error ValidationFailed => Error.Validation(code: "Jwt.ValidationFailed", description: "Token validation failed");
        public static Error ParseFailed => Error.Failure(code: "Jwt.ParseFailed", description: "Failed to parse token");
        public static Error GenerationFailed => Error.Failure(code: "Jwt.GenerationFailed", description: "Failed to generate JWT token");
        public static Error InvalidUser => Error.Validation(code: "Jwt.InvalidUser", description: "Valid user is required");
    }
}