using System.Linq.Expressions;
using System.Text;

using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Models;

namespace ReSys.Core.Common.Data;

/// <summary>
/// A fluent builder for constructing complex QueryOptions in a type-safe manner.
/// Supports property mappings, logical grouping, dynamic filtering, sorting, and global search.
/// </summary>
/// <example>
/// <code>
/// var options = new QueryBuilder&lt;Product&gt;()
///     .AddMap("cat", x => x.Category!.Name)
///     .StartGroup()
///         .Where(x => x.Price, ">", 100)
///         .Or()
///         .Where("cat", "=", "Premium")
///     .EndGroup()
///     .OrderByDescending(x => x.CreatedAt)
///     .Search("phone", x => x.Name, x => x.Description!)
///     .Page(1, 10)
///     .Build();
/// </code>
/// </example>
/// <typeparam name="T">The entity type to build the query for.</typeparam>
public class QueryBuilder<T>
{
    private readonly StringBuilder _filterSb = new();
    private readonly List<string> _sorts = new();
    private string? _searchText;
    private readonly List<string> _SearchField = new();
    private int? _page;
    private int? _pageSize;
    private readonly Dictionary<string, string> _mappings = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a custom mapping for a field name.
    /// Allows using shorthand or alternate names in the builder that translate to actual entity properties.
    /// </summary>
    /// <example>AddMap("cat", "Category.Name") allows .Where("cat", "=", "Books")</example>
    public QueryBuilder<T> AddMap(string from, string to)
    {
        _mappings[from] = to;
        return this;
    }

    /// <summary>
    /// Adds a custom mapping using an expression for the target property.
    /// </summary>
    public QueryBuilder<T> AddMap(string from, Expression<Func<T, object>> propertySelector)
    {
        return AddMap(from, GetPropertyName(propertySelector));
    }

    /// <summary>
    /// Adds a filter condition.
    /// </summary>
    /// <param name="property">The property name (or mapped shorthand).</param>
    /// <param name="op">The operator (e.g., "=", ">", "*").</param>
    /// <param name="value">The value to compare against.</param>
    public QueryBuilder<T> Where(string property, string op, object? value)
    {
        AppendSeparator();

        // Apply Mapping if exists
        if (_mappings.TryGetValue(property, out var mapped))
        {
            property = mapped;
        }

        _filterSb.Append($"{property}{op}{FormatValue(value)}");
        return this;
    }

    /// <summary>
    /// Adds a filter condition using a type-safe property selector.
    /// </summary>
    public QueryBuilder<T> Where(Expression<Func<T, object>> propertySelector, string op, object? value)
    {
        var propertyName = GetPropertyName(propertySelector);
        return Where(propertyName, op, value);
    }

    /// <summary>
    /// Adds a logical OR operator between the previous and next conditions.
    /// Note: OR has lower precedence than the default AND (comma) operator.
    /// </summary>
    public QueryBuilder<T> Or()
    {
        if (_filterSb.Length > 0)
        {
            _filterSb.Append("|");
        }
        return this;
    }

    /// <summary>
    /// Starts a logical grouping (parenthesis) for filters.
    /// </summary>
    public QueryBuilder<T> StartGroup()
    {
        AppendSeparator();
        _filterSb.Append("(");
        return this;
    }

    /// <summary>
    /// Ends a logical grouping (parenthesis).
    /// </summary>
    public QueryBuilder<T> EndGroup()
    {
        _filterSb.Append(")");
        return this;
    }

    /// <summary>
    /// Appends a raw filter string directly to the output.
    /// </summary>
    public QueryBuilder<T> AddRawFilter(string filter)
    {
        if (!string.IsNullOrWhiteSpace(filter))
        {
            AppendSeparator();
            _filterSb.Append(filter);
        }
        return this;
    }

    /// <summary>
    /// Adds an ascending sort by the specified property.
    /// </summary>
    public QueryBuilder<T> OrderBy(string property)
    {
        if (_mappings.TryGetValue(property, out var mapped)) property = mapped;
        _sorts.Add(property);
        return this;
    }

    /// <summary>
    /// Adds a descending sort by the specified property.
    /// </summary>
    public QueryBuilder<T> OrderByDescending(string property)
    {
        if (_mappings.TryGetValue(property, out var mapped)) property = mapped;
        _sorts.Add($"{property} desc");
        return this;
    }

    /// <summary>
    /// Adds an ascending sort using a type-safe property selector.
    /// </summary>
    public QueryBuilder<T> OrderBy(Expression<Func<T, object>> propertySelector)
    {
        return OrderBy(GetPropertyName(propertySelector));
    }

    /// <summary>
    /// Adds a descending sort using a type-safe property selector.
    /// </summary>
    public QueryBuilder<T> OrderByDescending(Expression<Func<T, object>> propertySelector)
    {
        return OrderByDescending(GetPropertyName(propertySelector));
    }

    /// <summary>
    /// Configures global search for the query.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="fields">The properties to include in the search.</param>
    public QueryBuilder<T> Search(string searchText, params string[] fields)
    {
        _searchText = searchText;
        if (fields != null)
        {
            foreach (var f in fields)
            {
                var field = f;
                if (_mappings.TryGetValue(field, out var mapped)) field = mapped;
                _SearchField.Add(field);
            }
        }
        return this;
    }

    /// <summary>
    /// Configures global search using type-safe property selectors.
    /// </summary>
    public QueryBuilder<T> Search(string searchText, params Expression<Func<T, object>>[] fieldSelectors)
    {
        _searchText = searchText;
        if (fieldSelectors != null)
        {
            foreach (var selector in fieldSelectors)
            {
                _SearchField.Add(GetPropertyName(selector));
            }
        }
        return this;
    }

    /// <summary>
    /// Configures pagination parameters.
    /// </summary>
    public QueryBuilder<T> Page(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
        return this;
    }

    /// <summary>
    /// Performs a structural validation of the built query.
    /// Checks if all referenced properties (including nested paths) exist on the entity type T.
    /// </summary>
    /// <param name="errors">Output list containing descriptions of any invalid properties found.</param>
    /// <returns>True if all properties are valid; otherwise false.</returns>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();
        
        // 1. Check Sort fields
        foreach (var sort in _sorts)
        {
            var propPath = sort.Split(' ')[0]; // remove ' desc'
            if (!CheckPropertyPath(typeof(T), propPath))
            {
                errors.Add($"Invalid Sort Property: {propPath}");
            }
        }

        // 2. Check Search fields
        foreach (var search in _SearchField)
        {
            if (!CheckPropertyPath(typeof(T), search))
            {
                errors.Add($"Invalid Search Property: {search}");
            }
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Internal recursive helper to validate a dot-separated property path against a type.
    /// </summary>
    private bool CheckPropertyPath(Type type, string path)
    {
        var currentType = type;
        foreach (var part in path.Split('.'))
        {
            var property = QueryHelper.GetPropertyCaseInsensitive(currentType, part);
            if (property == null) return false;
            currentType = property.PropertyType;
        }
        return true;
    }

    /// <summary>
    /// Builds the final unified QueryOptions object.
    /// </summary>
    public QueryOptions Build()
    {
        return new QueryOptions
        {
            Filter = _filterSb.ToString(),
            Sort = _sorts.Count > 0 ? string.Join(",", _sorts) : null,
            Search = _searchText,
            SearchField = _SearchField.Count > 0 ? _SearchField.ToArray() : null,
            Page = _page,
            PageSize = _pageSize
        };
    }

    // Granular build methods for targeted consumption
    public FilterOptions BuildFilterOptions() => new() { Filter = _filterSb.ToString() };
    public SortOptions BuildSortOptions() => new() { Sort = _sorts.Count > 0 ? string.Join(",", _sorts) : null };
    public SearchOptions BuildSearchOptions() => new() { Search = _searchText, SearchField = _SearchField.Count > 0 ? _SearchField.ToArray() : null };
    public PageOptions BuildPageOptions() => new() { Page = _page, PageSize = _pageSize };

    /// <summary>
    /// Ensures proper comma separation between conditions while respecting groups and OR logic.
    /// </summary>
    private void AppendSeparator()
    {
        if (_filterSb.Length > 0)
        {
            char last = _filterSb[_filterSb.Length - 1];
            if (last != '(' && last != '|')
            {
                _filterSb.Append(",");
            }
        }
    }

    /// <summary>
    /// Resolves a full property path (e.g., "Category.Name") from a LINQ expression.
    /// </summary>
    private string GetPropertyName(Expression<Func<T, object>> propertySelector)
    {
        MemberExpression? member = null;
        if (propertySelector.Body is UnaryExpression unary)
        {
            member = unary.Operand as MemberExpression;
        }
        else
        {
            member = propertySelector.Body as MemberExpression;
        }

        if (member == null)
            throw new ArgumentException("Expression must be a member access", nameof(propertySelector));

        return GetFullPropertyName(member);
    }

    /// <summary>
    /// Recursively traverses a MemberExpression to build the full dot-separated path.
    /// </summary>
    private string GetFullPropertyName(MemberExpression memberExpr)
    {
        if (memberExpr.Expression is MemberExpression parent)
        {
            return $"{GetFullPropertyName(parent)}.{memberExpr.Member.Name}";
        }
        return memberExpr.Member.Name;
    }

    /// <summary>
    /// Formats values into their DSL string representation.
    /// Booleans become lowercase, Dates/Offsets use ISO 8601, and nulls become "null".
    /// </summary>
    private string FormatValue(object? value)
    {
        if (value == null) return "null";
        if (value is bool b) return b.ToString().ToLower();
        if (value is DateTime dt) return dt.ToString("o");
        if (value is DateTimeOffset dto) return dto.ToString("o");
        if (value is string s) return s; 
        return value.ToString()!;
    }
}
