using System.Linq.Expressions;
using System.Text;

using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Models;

namespace ReSys.Core.Common.Data;

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
    /// e.g. AddMap("qa", "Quantity") allows filtering by "qa=5".
    /// </summary>
    public QueryBuilder<T> AddMap(string from, string to)
    {
        _mappings[from] = to;
        return this;
    }

    public QueryBuilder<T> AddMap(string from, Expression<Func<T, object>> propertySelector)
    {
        return AddMap(from, GetPropertyName(propertySelector));
    }

    public QueryBuilder<T> Where(string property, string op, object? value)
    {
        AppendSeparator();

        // Apply Mapping
        if (_mappings.TryGetValue(property, out var mapped))
        {
            property = mapped;
        }

        _filterSb.Append($"{property}{op}{FormatValue(value)}");
        return this;
    }

    public QueryBuilder<T> Where(Expression<Func<T, object>> propertySelector, string op, object? value)
    {
        var propertyName = GetPropertyName(propertySelector);
        return Where(propertyName, op, value);
    }

    /// <summary>
    /// Adds a logical OR operator. 
    /// Note: OR has lower precedence than AND (comma).
    /// </summary>
    public QueryBuilder<T> Or()
    {
        if (_filterSb.Length > 0)
        {
            _filterSb.Append("|");
        }
        return this;
    }

    public QueryBuilder<T> StartGroup()
    {
        AppendSeparator();
        _filterSb.Append("(");
        return this;
    }

    public QueryBuilder<T> EndGroup()
    {
        _filterSb.Append(")");
        return this;
    }

    public QueryBuilder<T> AddRawFilter(string filter)
    {
        if (!string.IsNullOrWhiteSpace(filter))
        {
            AppendSeparator();
            _filterSb.Append(filter);
        }
        return this;
    }

    public QueryBuilder<T> OrderBy(string property)
    {
        if (_mappings.TryGetValue(property, out var mapped)) property = mapped;
        _sorts.Add(property);
        return this;
    }

    public QueryBuilder<T> OrderByDescending(string property)
    {
        if (_mappings.TryGetValue(property, out var mapped)) property = mapped;
        _sorts.Add($"{property} desc");
        return this;
    }

    public QueryBuilder<T> OrderBy(Expression<Func<T, object>> propertySelector)
    {
        return OrderBy(GetPropertyName(propertySelector));
    }

    public QueryBuilder<T> OrderByDescending(Expression<Func<T, object>> propertySelector)
    {
        return OrderByDescending(GetPropertyName(propertySelector));
    }

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

    public QueryBuilder<T> Page(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
        return this;
    }

    /// <summary>
    /// Checks if all mapped/used properties exist on type T.
    /// Does not validate values or operators.
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();
        // Validation logic:
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

    public FilterOptions BuildFilterOptions() => new() { Filter = _filterSb.ToString() };
    public SortOptions BuildSortOptions() => new() { Sort = _sorts.Count > 0 ? string.Join(",", _sorts) : null };
    public SearchOptions BuildSearchOptions() => new() { Search = _searchText, SearchField = _SearchField.Count > 0 ? _SearchField.ToArray() : null };
    public PageOptions BuildPageOptions() => new() { Page = _page, PageSize = _pageSize };

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

    private string GetFullPropertyName(MemberExpression memberExpr)
    {
        if (memberExpr.Expression is MemberExpression parent)
        {
            return $"{GetFullPropertyName(parent)}.{memberExpr.Member.Name}";
        }
        return memberExpr.Member.Name;
    }

        private string FormatValue(object? value)
        {
            if (value == null) return "null";
            if (value is bool b) return b.ToString().ToLower();
            if (value is DateTime dt) return dt.ToString("o");
            if (value is DateTimeOffset dto) return dto.ToString("o");
            if (value is string s) return s; 
            return value.ToString()!;
        }}