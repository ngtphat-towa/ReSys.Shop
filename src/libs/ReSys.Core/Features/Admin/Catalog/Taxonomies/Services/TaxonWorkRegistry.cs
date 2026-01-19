namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;

/// <summary>
/// Scoped registry to collect and deduplicate taxon-related background work during a single request.
/// </summary>
public interface ITaxonWorkRegistry
{
    void RegisterHierarchyRebuild(Guid taxonomyId);
    void RegisterProductRegeneration(Guid taxonId);
    IReadOnlyCollection<Guid> GetPendingHierarchyRebuilds();
    IReadOnlyCollection<Guid> GetPendingProductRegenerations();
}

public class TaxonWorkRegistry : ITaxonWorkRegistry
{
    private readonly HashSet<Guid> _pendingHierarchyRebuilds = [];
    private readonly HashSet<Guid> _pendingProductRegenerations = [];

    public void RegisterHierarchyRebuild(Guid taxonomyId) => _pendingHierarchyRebuilds.Add(taxonomyId);
    public void RegisterProductRegeneration(Guid taxonId) => _pendingProductRegenerations.Add(taxonId);
    
    public IReadOnlyCollection<Guid> GetPendingHierarchyRebuilds() => _pendingHierarchyRebuilds.ToList().AsReadOnly();
    public IReadOnlyCollection<Guid> GetPendingProductRegenerations() => _pendingProductRegenerations.ToList().AsReadOnly();
}
