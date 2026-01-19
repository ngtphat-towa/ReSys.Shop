using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Tokens;

public static class RefreshTokenEvents
{
    public record TokenGenerated(RefreshToken Token) : IDomainEvent;
    public record TokenRevoked(RefreshToken Token, string Reason) : IDomainEvent;
    public record TokenRotated(RefreshToken OldToken, RefreshToken NewToken) : IDomainEvent;
}