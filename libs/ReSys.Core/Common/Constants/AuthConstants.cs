namespace ReSys.Core.Common.Constants;

public static class AuthConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Customer = "Customer";
    }

    public static class Clients
    {
        public const string ShopWeb = "resys-shop-web";
        public const string AdminWeb = "resys-admin-web";
    }

    public static class Scopes
    {
        public const string Roles = "roles";
        public const string Permissions = "permissions";
    }

    public static class Resources
    {
        public const string ShopApi = "resource_server";
    }

    public static class Schemes
    {
        public const string Bearer = "OpenIddict.Validation.AspNetCore";
    }

    public static class DevelopmentPorts
    {
        public const int Identity = 5003;
        public const int Api = 5001;
        public const int Gateway = 5002;
        public const int ML = 8000;
        public const int Shop = 5173;
        public const int Admin = 5174;

        public static string IdentityUrl => $"https://localhost:{Identity}";
        public static string ApiUrl => $"https://localhost:{Api}";
    }
}
