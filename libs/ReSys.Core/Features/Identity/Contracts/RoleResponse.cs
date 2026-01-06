namespace ReSys.Core.Features.Identity.Contracts;

public record RoleResponse(
    string Id, 
    string Name, 
    IEnumerable<string> Permissions);
