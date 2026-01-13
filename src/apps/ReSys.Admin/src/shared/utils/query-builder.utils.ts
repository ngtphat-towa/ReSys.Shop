/**
 * Supported filter operators for the dynamic query engine.
 * = (Equals), != (Not Equals), > (Greater), < (Less), >= (GreaterOrEqual), <= (LessOrEqual)
 * * (Contains), !* (Not Contains), ^ (StartsWith), $ (EndsWith)
 */
export type FilterOperator = '=' | '!=' | '>' | '<' | '>=' | '<=' | '!*' | '*' | '^' | '$';

/**
 * Helper type to extract nested keys from an object as string paths.
 * e.g., 'address.city' for { address: { city: string } }
 */
export type NestedKeyOf<T extends object> = {
  [K in keyof T & (string | number)]: T[K] extends object
    ? `${K}` | `${K}.${NestedKeyOf<T[K]>}`
    : `${K}`;
}[keyof T & (string | number)];

/**
 * TypeScript version of the backend QueryBuilder.
 * Helps construct complex filter, sort, and pagination parameters for API requests.
 * 
 * @example
 * ```typescript
 * const params = new QueryBuilder<Product>()
 *   .where('price', '>', 100)
 *   .orderByDescending('createdAt')
 *   .page(1, 20)
 *   .build();
 * 
 * // Output: { filter: "price>100", sort: "createdAt desc", page: 1, page_size: 20 }
 * ```
 */
export class QueryBuilder<T extends object = Record<string, unknown>> {
  private _filterParts: string[] = [];
  private _sorts: string[] = [];
  private _searchText?: string;
  private _searchFields: string[] = [];
  private _page?: number;
  private _pageSize?: number;
  private _mappings: Map<string, string> = new Map();

  /**
   * Adds a custom mapping for a field name.
   * Allows using shorthand or alternate names in the builder.
   * 
   * @example
   * ```typescript
   * builder.addMap('cat', 'category.name').where('cat', '=', 'Books');
   * ```
   * @param from Shorthand name
   * @param to Actual property path
   */
  addMap(from: string, to: NestedKeyOf<T> | string): this {
    this._mappings.set(from, to as string);
    return this;
  }

  /**
   * Adds a filter condition. Multiple calls are joined by AND (comma) by default.
   * Supports nested properties and automatic formatting of Dates and Booleans.
   * 
   * @example
   * ```typescript
   * builder.where('name', '*', 'apple').where('category.id', '=', someGuid);
   * ```
   * @param field Property name or mapped name
   * @param operator Filter operator
   * @param value Value to filter by (null results in "null" string)
   */
  where(field: NestedKeyOf<T> | string, operator: FilterOperator, value: unknown): this {
    if (value === undefined || value === '') return this;

    this.appendSeparator();
    const mappedField = this._mappings.get(field as string) || field;
    this._filterParts.push(`${mappedField}${operator}${this.formatValue(value)}`);
    return this;
  }

  /**
   * Adds a logical OR operator between filter conditions.
   * Note: OR has lower precedence than the default AND (comma) operator.
   * 
   * @example
   * ```typescript
   * builder.where('status', '=', 'Active').or().where('price', '<', 10);
   * ```
   */
  or(): this {
    if (this._filterParts.length > 0) {
      this._filterParts.push('|');
    }
    return this;
  }

  /**
   * Starts a logical grouping (parenthesis) in the filter.
   * 
   * @example
   * ```typescript
   * builder.startGroup().where('a', '=', 1).or().where('b', '=', 2).endGroup();
   * ```
   */
  startGroup(): this {
    this.appendSeparator();
    this._filterParts.push('(');
    return this;
  }

  /**
   * Ends a logical grouping (parenthesis) in the filter.
   */
  endGroup(): this {
    this._filterParts.push(')');
    return this;
  }

  /**
   * Appends a raw filter string to the current query.
   * 
   * @example
   * ```typescript
   * builder.addRaw("CustomFunction(Price, 10)");
   * ```
   */
  addRaw(filter: string): this {
    if (filter) {
      this.appendSeparator();
      this._filterParts.push(filter);
    }
    return this;
  }

  /**
   * Adds a sort condition.
   * 
   * @example
   * ```typescript
   * builder.orderBy('price', 'desc').orderBy('name', 'asc');
   * ```
   * @param field Property name or mapped name
   * @param direction 'asc' or 'desc' (default: 'asc')
   */
  orderBy(field: NestedKeyOf<T> | string, direction: 'asc' | 'desc' = 'asc'): this {
    const mappedField = this._mappings.get(field as string) || field;
    if (direction === 'desc') {
      this._sorts.push(`${mappedField} desc`);
    } else {
      this._sorts.push(mappedField as string);
    }
    return this;
  }

  /**
   * Shorthand for adding a descending sort condition.
   */
  orderByDescending(field: NestedKeyOf<T> | string): this {
    return this.orderBy(field, 'desc');
  }

  /**
   * Configures global search.
   * 
   * @example
   * ```typescript
   * builder.search('iphone', ['name', 'description', 'category.name']);
   * ```
   * @param text Search term
   * @param fields Fields to include in search (supports nested paths)
   */
  search(text: string, fields: (NestedKeyOf<T> | string)[]): this {
    if (!text) return this;
    this._searchText = text;
    this._searchFields = fields.map((f) => this._mappings.get(f as string) || f) as string[];
    return this;
  }

  /**
   * Configures 1-based pagination.
   * 
   * @param index 1-based page index
   * @param size Number of items per page
   */
  page(index: number, size: number): this {
    this._page = index;
    this._pageSize = size;
    return this;
  }

  /**
   * Builds the final query parameters object.
   * Keys are snake_case to be handled by the backend normalization middleware.
   * 
   * @returns An object like `{ filter?: string, sort?: string, search?: string, search_field?: string[], page?: number, page_size?: number }`
   */
  build(): {
    filter?: string
    sort?: string
    search?: string
    search_field?: string[]
    page?: number
    page_size?: number
  } {
    const params: Record<string, unknown> = {};

    if (this._filterParts.length > 0) {
      params.filter = this._filterParts.join('');
    }

    if (this._sorts.length > 0) {
      params.sort = this._sorts.join(',');
    }

    if (this._searchText) {
      params.search = this._searchText;
      if (this._searchFields.length > 0) {
        params.search_field = this._searchFields;
      }
    }

    if (this._page !== undefined) params.page = this._page;
    if (this._pageSize !== undefined) params.page_size = this._pageSize;

    return params;
  }

  /**
   * Returns only the filter string part of the builder.
   * Useful for internal debugging or complex compositions.
   */
  buildFilterString(): string {
    return this._filterParts.join('');
  }

  private appendSeparator(): void {
    if (this._filterParts.length > 0) {
      const last = this._filterParts[this._filterParts.length - 1];
      if (last !== '(' && last !== '|') {
        this._filterParts.push(',');
      }
    }
  }

  private formatValue(value: unknown): string {
    if (value === null || value === undefined) return 'null';
    if (value instanceof Date) return value.toISOString();
    return String(value);
  }
}
