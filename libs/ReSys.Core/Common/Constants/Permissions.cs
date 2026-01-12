namespace ReSys.Core.Common.Constants;

public record PermissionMetadata(string Type, string Value, string Description, string Category);

public static class AppPermissions
{
    public static class Identity
    {
        public const string Group = "identity";

        public static class Users
        {
            public const string View = "identity.users.view";
            public const string List = "identity.users.list";
            public const string Create = "identity.users.create";
            public const string Edit = "identity.users.edit";
            public const string Delete = "identity.users.delete";
            public const string ManagePermissions = "identity.users.permissions.manage";
            public const string ManageRoles = "identity.users.roles.manage";
        }

        public static class Roles
        {
            public const string View = "identity.roles.view";
            public const string List = "identity.roles.list";
            public const string Create = "identity.roles.create";
            public const string Edit = "identity.roles.edit";
            public const string Delete = "identity.roles.delete";
            public const string ManagePermissions = "identity.roles.permissions.manage";
        }

        public static class PermissionsManagement
        {
            public const string View = "identity.permissions.view";
            public const string List = "identity.permissions.list";
            public const string Create = "identity.permissions.create";
            public const string Edit = "identity.permissions.edit";
            public const string Delete = "identity.permissions.delete";
        }
    }

    public static class Testing
    {
        public static class ExampleCategories
        {
            public const string View = "testing.example_categories.view";
            public const string List = "testing.example_categories.list";
            public const string Create = "testing.example_categories.create";
            public const string Edit = "testing.example_categories.edit";
            public const string Delete = "testing.example_categories.delete";
        }
    }

    public static IReadOnlyList<PermissionMetadata> All { get; } = new List<PermissionMetadata>
    {
        // Users
        new("permission", Identity.Users.View, "View user details", "Identity"),
        new("permission", Identity.Users.List, "List users", "Identity"),
        new("permission", Identity.Users.Create, "Create user", "Identity"),
        new("permission", Identity.Users.Edit, "Edit user", "Identity"),
        new("permission", Identity.Users.Delete, "Delete user", "Identity"),
        new("permission", Identity.Users.ManagePermissions, "Manage user permissions", "Identity"),
        new("permission", Identity.Users.ManageRoles, "Manage user roles", "Identity"),

        // Roles
        new("permission", Identity.Roles.View, "View role details", "Identity"),
        new("permission", Identity.Roles.List, "List roles", "Identity"),
        new("permission", Identity.Roles.Create, "Create role", "Identity"),
        new("permission", Identity.Roles.Edit, "Edit role", "Identity"),
        new("permission", Identity.Roles.Delete, "Delete role", "Identity"),
        new("permission", Identity.Roles.ManagePermissions, "Manage role permissions", "Identity"),

        // Permissions
        new("permission", Identity.PermissionsManagement.View, "View permission details", "Identity"),
        new("permission", Identity.PermissionsManagement.List, "List permissions", "Identity"),
        new("permission", Identity.PermissionsManagement.Create, "Create permission", "Identity"),
        new("permission", Identity.PermissionsManagement.Edit, "Edit permission", "Identity"),
        new("permission", Identity.PermissionsManagement.Delete, "Delete permission", "Identity"),

        // Testing - Example Categories
        new("permission", Testing.ExampleCategories.View, "View example category details", "Testing"),
        new("permission", Testing.ExampleCategories.List, "List example categories", "Testing"),
        new("permission", Testing.ExampleCategories.Create, "Create example category", "Testing"),
        new("permission", Testing.ExampleCategories.Edit, "Edit example category", "Testing"),
        new("permission", Testing.ExampleCategories.Delete, "Delete example category", "Testing"),
    }.AsReadOnly();
}
