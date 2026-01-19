using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Inventories.Locations;
using ErrorOr;
using System.Text.RegularExpressions;

namespace ReSys.Core.Domain.Settings.Stores;

public sealed class Store : Aggregate, IHasMetadata, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    // Financial Rules
    public string DefaultCurrency { get; set; } = "USD";
    public bool PricesIncludeTax { get; set; } = false;

    // Logistics
    public string DefaultWeightUnit { get; set; } = "kg";
    public Guid? DefaultStockLocationId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

        // Relationships
        private readonly List<StoreStockLocation> _storeStockLocations = [];
        public IReadOnlyCollection<StoreStockLocation> StoreStockLocations => _storeStockLocations.AsReadOnly();
    
        private Store() { }
    
        public static ErrorOr<Store> Create(
            string name, 
            string code, 
            string currency,
            string url = "",
            bool pricesIncludeTax = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return StoreErrors.NameRequired;
            if (string.IsNullOrWhiteSpace(code)) return StoreErrors.CodeRequired;
            if (string.IsNullOrWhiteSpace(currency)) return StoreErrors.CurrencyRequired;
    
            var normalizedCode = code.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(normalizedCode, StoreConstraints.CodeRegex))
                return StoreErrors.InvalidCodeFormat;
    
            var store = new Store
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Code = normalizedCode,
                DefaultCurrency = currency.Trim().ToUpperInvariant(),
                Url = url.Trim(),
                PricesIncludeTax = pricesIncludeTax
            };
    
            store.RaiseDomainEvent(new StoreEvents.StoreCreated(store));
            return store;
        }
    
        public ErrorOr<Success> Update(
            string name, 
            string url, 
            string currency, 
            bool pricesIncludeTax,
            string weightUnit)
        {
            if (string.IsNullOrWhiteSpace(name)) return StoreErrors.NameRequired;
            if (string.IsNullOrWhiteSpace(currency)) return StoreErrors.CurrencyRequired;
    
            Name = name.Trim();
            Url = url.Trim();
            DefaultCurrency = currency.Trim().ToUpperInvariant();
            PricesIncludeTax = pricesIncludeTax;
            DefaultWeightUnit = weightUnit.Trim();
    
            RaiseDomainEvent(new StoreEvents.StoreUpdated(this));
            return Result.Success;
        }
    
        public void SetDefaultLocation(Guid locationId)
        {
            if (!_storeStockLocations.Any(x => x.StockLocationId == locationId))
            {
                // Business Rule: Can only default to a location we actually use.
                return; 
            }
            
            DefaultStockLocationId = locationId;
            RaiseDomainEvent(new StoreEvents.StoreUpdated(this));
        }
    
        public ErrorOr<Success> AddLocation(StockLocation location, bool isActive = true, int priority = 0)
        {
            if (_storeStockLocations.Any(x => x.StockLocationId == location.Id))
                return Result.Success;
    
            var link = StoreStockLocation.Create(Id, location.Id, isActive, priority);
            _storeStockLocations.Add(link);
            
            RaiseDomainEvent(new StoreEvents.StoreUpdated(this));
            return Result.Success;
        }
    
            public ErrorOr<Success> RemoveLocation(Guid locationId)
            {
                if (DefaultStockLocationId == locationId)
                    return StoreErrors.CannotRemoveDefaultLocation;
        
                var link = _storeStockLocations.FirstOrDefault(x => x.StockLocationId == locationId);            if (link == null) return Result.Success;
    
            _storeStockLocations.Remove(link);
            RaiseDomainEvent(new StoreEvents.StoreUpdated(this));
            return Result.Success;
        }
    public void SetMetadata(IDictionary<string, object?> publicMetadata, IDictionary<string, object?> privateMetadata)
    {
        PublicMetadata = publicMetadata;
        PrivateMetadata = privateMetadata;
    }

    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new StoreEvents.StoreDeleted(this));
        return Result.Deleted;
    }
}
