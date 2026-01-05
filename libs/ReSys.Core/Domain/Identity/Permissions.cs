using System.Collections.ObjectModel;

namespace ReSys.Core.Domain.Identity;

public static class Permissions
{
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Manage = "Permissions.Users.Manage";
    }
    
    public static IEnumerable<string> All()
    {
        yield return Products.View;
        yield return Products.Create;
        yield return Products.Edit;
        yield return Products.Delete;
        yield return Users.View;
        yield return Users.Manage;
    }
}
