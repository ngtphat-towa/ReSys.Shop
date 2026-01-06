using ReSys.Core.Domain.Identity;

namespace ReSys.Core.Features.Identity.Contracts;

public record CreateUserRequest(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName, 
    UserType UserType);
