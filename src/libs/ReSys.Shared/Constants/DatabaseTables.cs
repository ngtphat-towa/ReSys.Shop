namespace ReSys.Shared.Constants;

public static class DatabaseTables
{
    public const string Users = "users";
    public const string Roles = "roles";
    public const string CustomerProfiles = "customer_profiles";
    public const string StaffProfiles = "staff_profiles";
    public const string UserGroups = "user_groups";
    public const string UserGroupMemberships = "user_group_memberships";
    public const string UserAddresses = "user_addresses";
    public const string AccessPermissions = "access_permissions";
    
    // Location
    public const string Countries = "countries";
    public const string States = "states";

    // Settings
    public const string Settings = "settings";
    public const string PaymentMethods = "payment_methods";
    public const string ShippingMethods = "shipping_methods";
    
    // Catalog
    public const string Products = "products";
    public const string Variants = "variants";
    public const string ProductProperties = "product_properties";
    public const string ProductImages = "product_images";
    public const string ProductImageEmbeddings = "product_image_embeddings";
    public const string OptionTypes = "option_types";
    public const string OptionValues = "option_values";
    public const string PropertyTypes = "property_types";
    public const string Taxonomies = "taxonomies";
    public const string Taxa = "taxa";
    public const string TaxonRules = "taxon_rules";
    public const string Classifications = "classifications";

    // Testing
    public const string Examples = "examples";
    public const string ExampleCategories = "example_categories";
    public const string ExampleEmbeddings = "example_embeddings";
}
