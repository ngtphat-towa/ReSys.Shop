namespace ReSys.Shared.Constants.Permissions;

public static class FeaturePermissions
{
    public static class Admin
    {
        public static class Identity
        {
            public static class User
            {
                public const string View = "admin.identity.user.view";
                public const string List = "admin.identity.user.list";
                public const string Create = "admin.identity.user.create";
                public const string Update = "admin.identity.user.update";
                public const string Delete = "admin.identity.user.delete";
                public const string AssignRole = "admin.identity.user.assign_role";
                public const string UnassignRole = "admin.identity.user.unassign_role";
            }

            public static class Role
            {
                public const string View = "admin.identity.role.view";
                public const string List = "admin.identity.role.list";
                public const string Create = "admin.identity.role.create";
                public const string Update = "admin.identity.role.update";
                public const string Delete = "admin.identity.role.delete";
                public const string AssignPermission = "admin.identity.role.assign_permission";
                public const string UnassignPermission = "admin.identity.role.unassign_permission";
            }

            public static class AccessControl
            {
                public const string View = "admin.identity.permission.view";
                public const string List = "admin.identity.permission.list";
            }
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
