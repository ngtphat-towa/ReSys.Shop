using Microsoft.AspNetCore.Identity;

namespace ReSys.Core.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    // Distinguish between Staff and Customers
    public UserType UserType { get; set; } = UserType.Customer;
}

public enum UserType
{
    Customer = 0,
    Staff = 1
}
