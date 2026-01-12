using System.Collections.Immutable;

namespace ReSys.Core.Common.Interfaces;

public interface IUserContext
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IImmutableList<string> Roles { get; }
    IImmutableList<string> Permissions { get; }
    
    bool HasRole(string role);
    bool HasPermission(string permission);
    bool IsInAdminRole();
}
