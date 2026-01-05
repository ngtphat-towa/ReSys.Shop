using ReSys.Core.Domain.Identity;

namespace ReSys.Identity.Features.Account.Contracts;

public record CreateUserRequest(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName, 
    UserType UserType);
