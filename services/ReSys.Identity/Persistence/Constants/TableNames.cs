namespace ReSys.Identity.Persistence.Constants;

public static class Schemas
{
    public const string Identity = "identity";
}

public static class TableNames
{
    public const string Users = "Users";
    public const string Roles = "Roles";
    public const string UserRoles = "UserRoles";
    public const string UserClaims = "UserClaims";
    public const string UserLogins = "UserLogins";
    public const string RoleClaims = "RoleClaims";
    public const string UserTokens = "UserTokens";
    
    public const string ClaimDefinitions = "ClaimDefinitions";

    public static class OpenIddict
    {
        public const string Applications = "Applications";
        public const string Authorizations = "Authorizations";
        public const string Scopes = "Scopes";
        public const string Tokens = "Tokens";
    }
}
