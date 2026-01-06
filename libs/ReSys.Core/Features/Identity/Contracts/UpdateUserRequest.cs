namespace ReSys.Core.Features.Identity.Contracts;

public record UpdateUserRequest(
    string? FirstName, 
    string? LastName);
