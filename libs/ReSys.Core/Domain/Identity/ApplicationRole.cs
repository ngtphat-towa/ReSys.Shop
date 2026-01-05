using Microsoft.AspNetCore.Identity;

namespace ReSys.Core.Domain.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base() { }
    
    public ApplicationRole(string roleName) : base(roleName) { }
}
