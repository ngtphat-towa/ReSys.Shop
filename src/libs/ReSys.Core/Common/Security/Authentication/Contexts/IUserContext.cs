namespace ReSys.Core.Common.Security.Authentication.Contexts;

public interface IUserContext
{
    string? UserId { get; }
    string? AdhocCustomerId { get; }
    Guid? StoreId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
    void SetAdhocCustomerId(string adhocCustomerId);
}
