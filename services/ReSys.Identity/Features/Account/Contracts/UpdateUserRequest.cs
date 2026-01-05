namespace ReSys.Identity.Features.Account.Contracts;

public record UpdateUserRequest(
    string? FirstName, 
    string? LastName);
