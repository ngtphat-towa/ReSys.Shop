namespace ReSys.Identity.Features.Account.Contracts;

public record RoleResponse(
    string Id, 
    string Name, 
    IEnumerable<string> Permissions);
