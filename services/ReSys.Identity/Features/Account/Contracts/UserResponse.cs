namespace ReSys.Identity.Features.Account.Contracts;

public record UserResponse(
    string Id, 
    string UserName, 
    string Email, 
    string FirstName, 
    string LastName, 
    string UserType,
    IEnumerable<string> Roles);
