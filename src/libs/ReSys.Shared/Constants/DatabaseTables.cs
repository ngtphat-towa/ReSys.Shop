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
    public const string RefreshTokens = "refresh_tokens";

    // Identity Internal
    public const string UserRoles = "user_roles";
    public const string UserClaims = "user_claims";
    public const string UserLogins = "user_logins";
    public const string RoleClaims = "role_claims";
    public const string UserTokens = "user_tokens";
    
    // Location
    public const string Countries = "countries";
    public const string States = "states";

    // Settings
    public const string Settings = "settings";
    public const string Stores = "stores";
    public const string StoreStockLocations = "store_stock_locations";
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

    // Inventory
    public const string StockLocations = "stock_locations";
    public const string StockItems = "stock_items";
    public const string InventoryUnits = "inventory_units";
    public const string StockMovements = "stock_movements";
    public const string StockTransfers = "stock_transfers";
    public const string StockTransferItems = "stock_transfer_items";

    // Ordering
    public const string Orders = "orders";
    public const string LineItems = "line_items";
    public const string OrderAdjustments = "order_adjustments";
    public const string LineItemAdjustments = "line_item_adjustments";
    public const string Payments = "payments";
    public const string Shipments = "shipments";
    public const string OrderHistories = "order_histories";

    // Testing
    public const string Examples = "examples";
    public const string ExampleCategories = "example_categories";
    public const string ExampleEmbeddings = "example_embeddings";
}
