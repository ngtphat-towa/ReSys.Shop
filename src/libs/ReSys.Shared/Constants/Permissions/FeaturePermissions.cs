namespace ReSys.Shared.Constants.Permissions;

public static class FeaturePermissions
{
    public static class Admin
    {
        public static class User
        {
            public const string View = "admin.user.view";
            public const string Create = "admin.user.create";
            public const string Edit = "admin.user.edit";
            public const string Delete = "admin.user.delete";
        }

        public static class Role
        {
            public const string View = "admin.role.view";
            public const string Create = "admin.role.create";
            public const string Edit = "admin.role.edit";
            public const string Delete = "admin.role.delete";
        }

        public static class Catalog
        {
            public const string View = "admin.catalog.view";
            public const string Create = "admin.catalog.create";
            public const string Edit = "admin.catalog.edit";
            public const string Delete = "admin.catalog.delete";
        }
    }
}
