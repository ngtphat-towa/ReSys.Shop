import { describe, it, expect } from 'vitest';
import { QueryBuilder } from './query-builder.utils';

interface TestItem {
  id: string;
  name: string;
  price: number;
  category: {
    id: string;
    name: string;
  };
}

describe('QueryBuilder', () => {
  it('should build basic filter, sort and page', () => {
    const builder = new QueryBuilder<TestItem>()
      .where('name', '=', 'Apple')
      .orderBy('price', 'desc')
      .page(2, 20);

    const params = builder.build();

    expect(params.filter).toBe('name=Apple');
    expect(params.sort).toBe('price desc');
    expect(params.page).toBe(2);
    expect(params.page_size).toBe(20);
  });

  it('should support multiple where conditions with commas', () => {
    const builder = new QueryBuilder<TestItem>()
      .where('price', '>', 10)
      .where('name', '*', 'fruit');

    expect(builder.build().filter).toBe('price>10,name*fruit');
  });

  it('should support OR logic and grouping', () => {
    // (price < 10 OR price > 100) AND name = Special
    const builder = new QueryBuilder<TestItem>()
      .startGroup()
      .where('price', '<', 10)
      .or()
      .where('price', '>', 100)
      .endGroup()
      .where('name', '=', 'Special');

    expect(builder.build().filter).toBe('(price<10|price>100),name=Special');
  });

  it('should handle nested properties in types and output', () => {
    const builder = new QueryBuilder<TestItem>()
      .where('category.name', '=', 'Books')
      .orderBy('category.name');

    const params = builder.build();
    expect(params.filter).toBe('category.name=Books');
    expect(params.sort).toBe('category.name');
  });

  it('should support search with multiple fields', () => {
    const builder = new QueryBuilder<TestItem>()
      .search('query', ['name', 'category.name']);

    const params = builder.build();
    expect(params.search).toBe('query');
    expect(params.search_field).toEqual(['name', 'category.name']);
  });

  it('should support custom mappings', () => {
    const builder = new QueryBuilder<TestItem>()
      .addMap('cat', 'category.name')
      .where('cat', '=', 'Electronics')
      .orderBy('cat', 'desc');

    const params = builder.build();
    expect(params.filter).toBe('category.name=Electronics');
    expect(params.sort).toBe('category.name desc');
  });

  it('should format special values correctly', () => {
    const date = new Date('2025-01-01T10:00:00Z');
    const builder = new QueryBuilder()
      .where('createdAt', '>', date)
      .where('isActive', '=', true)
      .where('deletedAt', '=', null);

    const params = builder.build();
    expect(params.filter).toBe('createdAt>2025-01-01T10:00:00.000Z,isActive=true,deletedAt=null');
  });

  it('should skip undefined or empty string values in where', () => {
    const builder = new QueryBuilder()
      .where('name', '=', 'Valid')
      .where('ignored', '=', undefined)
      .where('emptyString', '=', '');

    expect(builder.build().filter).toBe('name=Valid');
  });

  it('should support raw filters', () => {
    const builder = new QueryBuilder()
      .where('name', '=', 'A')
      .addRaw('custom_op(1,2)');

    expect(builder.build().filter).toBe('name=A,custom_op(1,2)');
  });

  it('should support orderByDescending shorthand', () => {
    const builder = new QueryBuilder<TestItem>()
      .orderByDescending('name')
      .orderByDescending('category.name');

    const params = builder.build();
    expect(params.sort).toBe('name desc,category.name desc');
  });

  it('should handle complex chaining with mixed operations', () => {
    const builder = new QueryBuilder<TestItem>()
      .where('price', '>', 10)
      .startGroup()
      .where('name', '*', 'a')
      .or()
      .where('name', '*', 'b')
      .endGroup()
      .orderByDescending('price')
      .page(1, 100);

    const params = builder.build();
    expect(params.filter).toBe('price>10,(name*a|name*b)');
    expect(params.sort).toBe('price desc');
    expect(params.page).toBe(1);
    expect(params.page_size).toBe(100);
  });

  it('should verify snake_case property paths in filter string', () => {
    const builder = new QueryBuilder()
      .where('category_name', '=', 'Electronics')
      .where('is_active', '=', true);

    expect(builder.build().filter).toBe('category_name=Electronics,is_active=true');
  });

  it('should handle boolean and date formatting in filters', () => {
    const date = new Date('2026-01-07T12:00:00Z');
    const builder = new QueryBuilder()
      .where('isActive', '=', false)
      .where('createdAt', '>', date);

    const filter = builder.buildFilterString();
    expect(filter).toContain('isActive=false');
    expect(filter).toContain('createdAt>2026-01-07T12:00:00.000Z');
  });

  it('should handle empty sorts and filters gracefully', () => {
    const builder = new QueryBuilder().page(1, 10);
    const params = builder.build();
    
    expect(params.filter).toBeUndefined();
    expect(params.sort).toBeUndefined();
    expect(params.page).toBe(1);
  });

  it('should not add multiple separators for empty groups or ORs', () => {
    const builder = new QueryBuilder()
      .or() // Should be ignored as first element
      .startGroup()
      .endGroup()
      .where('name', '=', 'A');

    expect(builder.build().filter).toBe('(),name=A');
  });

  it('should support mapping for search fields', () => {
    const builder = new QueryBuilder<TestItem>()
      .addMap('n', 'name')
      .addMap('cn', 'category.name')
      .search('test', ['n', 'cn']);

    const params = builder.build();
    expect(params.search_field).toEqual(['name', 'category.name']);
  });
});
