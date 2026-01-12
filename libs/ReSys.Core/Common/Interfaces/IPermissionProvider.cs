namespace ReSys.Core.Common.Interfaces;

public interface IPermissionProvider
{
    Task<IEnumerable<string>> GetPermissionsForUserAsync(string userId);
}
