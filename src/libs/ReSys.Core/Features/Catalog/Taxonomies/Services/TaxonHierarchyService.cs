using ErrorOr;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.Taxonomies.Services;

/// <inheritdoc cref="ITaxonHierarchyService"/>
public sealed class TaxonHierarchyService(
    IApplicationDbContext context,
    ILogger<TaxonHierarchyService> logger) : ITaxonHierarchyService
{
    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> RebuildHierarchyAsync(Guid taxonomyId, CancellationToken ct)
    {
        if (taxonomyId == Guid.Empty) return TaxonomyErrors.NotFound(taxonomyId);

        try
        {
            // 1. Load all taxons for this taxonomy once to ensure memory consistency
            var allTaxons = await context.Set<Taxon>()
                .Where(t => t.TaxonomyId == taxonomyId)
                .ToListAsync(ct);

            if (allTaxons.Count == 0) return Result.Success;

            var taxonomy = await context.Set<Taxonomy>().FirstOrDefaultAsync(x => x.Id == taxonomyId, ct);
            if (taxonomy == null) return TaxonomyErrors.NotFound(taxonomyId);

            var taxonDict = allTaxons.ToDictionary(t => t.Id);

            // 2. Validate Hierarchy
            var validateResult = ValidateHierarchyInternal(allTaxons, taxonDict);
            if (validateResult.IsError) return validateResult.Errors;

            // 3. Rebuild Nested Sets (In Memory)
            var nestedSetResult = RebuildNestedSetsInternal(allTaxons, taxonDict);
            if (nestedSetResult.IsError) return nestedSetResult.Errors;

            // 4. Regenerate Permalinks (In Memory, depends on Parent navigation and Lft order)
            // Sort by Lft ensures parents are processed before children
            var sortedTaxons = allTaxons.OrderBy(t => t.Lft).ToList();
            foreach (var taxon in sortedTaxons)
            {
                if (taxon.ParentId.HasValue && taxonDict.TryGetValue(taxon.ParentId.Value, out var parent))
                {
                    taxon.Parent = parent;
                }
                else
                {
                    taxon.Parent = null;
                }
                taxon.UpdatePermalink(taxonomy.Name);
            }

            // 5. Save all changes
            await context.SaveChangesAsync(ct);

            logger.LogInformation("Successfully rebuilt taxonomy {TaxonomyId} hierarchy", taxonomyId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rebuild taxonomy {TaxonomyId} hierarchy", taxonomyId);
            return TaxonErrors.HierarchyRebuildFailed;
        }
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> ValidateHierarchyAsync(Guid taxonomyId, CancellationToken ct)
    {
        var allTaxons = await context.Set<Taxon>()
            .Where(t => t.TaxonomyId == taxonomyId)
            .ToListAsync(ct);

        if (allTaxons.Count == 0) return Result.Success;
        var taxonDict = allTaxons.ToDictionary(t => t.Id);

        return ValidateHierarchyInternal(allTaxons, taxonDict);
    }

    private ErrorOr<Success> ValidateHierarchyInternal(List<Taxon> allTaxons, Dictionary<Guid, Taxon> taxonDict)
    {
        var visited = new HashSet<Guid>();
        var recStack = new HashSet<Guid>();
        foreach (var id in taxonDict.Keys)
        {
            if (!visited.Contains(id))
            {
                if (HasCycle(id, taxonDict, visited, recStack))
                    return TaxonErrors.CycleDetected;
            }
        }

        var roots = allTaxons.Where(t => t.ParentId == null).ToList();
        if (roots.Count > 1) return TaxonErrors.RootConflict;
        if (roots.Count == 0) return TaxonErrors.NoRoot;

        return Result.Success;
    }

    private bool HasCycle(Guid id, Dictionary<Guid, Taxon> dict, HashSet<Guid> visited, HashSet<Guid> stack)
    {
        visited.Add(id);
        stack.Add(id);

        if (dict[id].ParentId.HasValue)
        {
            var parentId = dict[id].ParentId!.Value;
            if (!dict.ContainsKey(parentId)) return false;

            if (!visited.Contains(parentId))
            {
                if (HasCycle(parentId, dict, visited, stack)) return true;
            }
            else if (stack.Contains(parentId)) return true;
        }

        stack.Remove(id);
        return false;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> RebuildNestedSetsAsync(Guid taxonomyId, CancellationToken ct)
    {
        var allTaxons = await context.Set<Taxon>()
            .Where(t => t.TaxonomyId == taxonomyId)
            .ToListAsync(ct);

        if (allTaxons.Count == 0) return Result.Success;
        var taxonDict = allTaxons.ToDictionary(t => t.Id);

        var result = RebuildNestedSetsInternal(allTaxons, taxonDict);
        if (result.IsError) return result.Errors;

        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    private ErrorOr<Success> RebuildNestedSetsInternal(List<Taxon> allTaxons, Dictionary<Guid, Taxon> taxonDict)
    {
        var lookup = allTaxons.ToLookup(t => t.ParentId);
        var root = allTaxons.FirstOrDefault(t => t.ParentId == null);
        if (root == null) return TaxonErrors.NoRoot;

        ComputeNestedSets(root, lookup, 0, 1);
        return Result.Success;
    }

    private int ComputeNestedSets(Taxon taxon, ILookup<Guid?, Taxon> lookup, int depth, int counter)
    {
        var lft = counter++;
        var children = lookup[taxon.Id].OrderBy(t => t.Position).ToList();

        foreach (var child in children)
        {
            counter = ComputeNestedSets(child, lookup, depth + 1, counter);
        }

        var rgt = counter++;
        taxon.Lft = lft;
        taxon.Rgt = rgt;
        taxon.Depth = depth;

        return counter;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> RegeneratePermalinksAsync(Guid taxonomyId, CancellationToken ct)
    {
        var taxonomy = await context.Set<Taxonomy>().FirstOrDefaultAsync(x => x.Id == taxonomyId, ct);
        if (taxonomy == null) return TaxonomyErrors.NotFound(taxonomyId);

        var allTaxons = await context.Set<Taxon>()
            .Where(t => t.TaxonomyId == taxonomyId)
            .OrderBy(t => t.Lft)
            .ToListAsync(ct);

        if (allTaxons.Count == 0) return Result.Success;
        var taxonDict = allTaxons.ToDictionary(t => t.Id);

        foreach (var taxon in allTaxons)
        {
            if (taxon.ParentId.HasValue && taxonDict.TryGetValue(taxon.ParentId.Value, out var parent))
            {
                taxon.Parent = parent;
            }
            else
            {
                taxon.Parent = null;
            }
            taxon.UpdatePermalink(taxonomy.Name);
        }

        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    /// <inheritdoc/>
    public async Task<TaxonTreeResponse> BuildTaxonTreeAsync(TaxonQueryOptions options, CancellationToken ct)
    {
        var dbQuery = context.Set<Taxon>().AsNoTracking();

        if (options.TaxonomyId != null && options.TaxonomyId.Length > 0)
            dbQuery = dbQuery.Where(t => options.TaxonomyId.Contains(t.TaxonomyId));

        if (!options.IncludeHidden)
            dbQuery = dbQuery.Where(t => !t.HideFromNav);

        if (options.MaxDepth.HasValue)
            dbQuery = dbQuery.Where(t => t.Depth <= options.MaxDepth.Value);

        var allNodes = await dbQuery
            .OrderBy(x => x.Lft)
            .Select(TaxonListItem.GetProjection<TaxonTreeItem>())
            .ToListAsync(ct);

        if (!allNodes.Any()) return new TaxonTreeResponse();

        if (options.IncludeLeavesOnly == true)
        {
            var leaves = allNodes.Where(n => !n.HasChildren).ToList();
            return new TaxonTreeResponse { Tree = leaves };
        }

        var nodesById = allNodes.ToDictionary(x => x.Id);
        var childrenLookup = allNodes.ToLookup(x => x.ParentId);

        List<TaxonTreeItem> breadcrumbs = [];
        TaxonTreeItem? focusedNode = null;
        HashSet<Guid> activePath = [];

        if (options.FocusedTaxonId.HasValue && nodesById.TryGetValue(options.FocusedTaxonId.Value, out focusedNode))
        {
            var current = focusedNode;
            while (current != null)
            {
                activePath.Add(current.Id);
                breadcrumbs.Insert(0, current);
                current = current.ParentId.HasValue && nodesById.TryGetValue(current.ParentId.Value, out var parent) ? parent : null;
            }
        }

        foreach (var node in allNodes)
        {
            node.IsInActivePath = activePath.Contains(node.Id);
            node.IsExpanded = node.IsInActivePath;
            node.Children = childrenLookup[node.Id].ToList();
        }

        var tree = allNodes.Where(x => x.ParentId == null).OrderBy(x => x.Position).ToList();

        return new TaxonTreeResponse
        {
            Tree = tree,
            Breadcrumbs = breadcrumbs,
            FocusedNode = focusedNode,
            FocusedSubtree = focusedNode
        };
    }

    /// <inheritdoc/>
    public async Task<PagedList<TaxonListItem>> GetFlatTaxonsAsync(TaxonQueryOptions options, CancellationToken ct)
    {
        var query = context.Set<Taxon>()
            .AsNoTracking();

        if (options.TaxonomyId != null && options.TaxonomyId.Length > 0)
            query = query.Where(t => options.TaxonomyId.Contains(t.TaxonomyId));

        if (!options.IncludeHidden)
            query = query.Where(t => !t.HideFromNav);

        if (options.MaxDepth.HasValue)
            query = query.Where(t => t.Depth <= options.MaxDepth.Value);

        if (options.IncludeLeavesOnly == true)
            query = query.Where(t => t.Rgt - t.Lft == 1);

        if (options.FocusedTaxonId.HasValue)
        {
            var focused = await context.Set<Taxon>().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == options.FocusedTaxonId.Value, ct);
            if (focused != null)
            {
                query = query.Where(t => t.TaxonomyId == focused.TaxonomyId && t.Lft >= focused.Lft && t.Lft <= focused.Rgt);
            }
        }

        query = query.ApplyFilter(options)
            .ApplySearch(options);

        var sortedQuery = query.ApplySort(options);
        if (ReferenceEquals(sortedQuery, query))
        {
            sortedQuery = query.OrderBy(x => x.Lft);
        }

        return await sortedQuery.ToPagedListAsync(TaxonListItem.GetProjection<TaxonListItem>(), options.Page, options.PageSize, ct);
    }
}