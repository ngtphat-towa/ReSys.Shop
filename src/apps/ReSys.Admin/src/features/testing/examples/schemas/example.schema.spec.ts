import { describe, it, expect } from 'vitest';
import { ExampleSchema } from './example.schema';
import { ExampleStatus } from '../types/example.types';

describe('ExampleSchema', () => {
  it('should validate a valid example', () => {
    const validData = {
      name: 'Valid Name',
      description: 'A valid description',
      price: 10.50,
      status: ExampleStatus.Active,
      hex_color: '#FF0000',
      category_id: null,
      image_url: null,
    };

    const result = ExampleSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should validate status using numeric enum values', () => {
    const validData = {
      name: 'Valid Name',
      price: 10,
      status: 0, // Draft
    };
    const result = ExampleSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should fail for invalid status', () => {
    const invalidData = {
      name: 'Valid Name',
      price: 10,
      status: 999, // Invalid status
    };
    const result = ExampleSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    
    if (result.success === false) {
      expect(result.error.issues[0]!.path).toContain('status');
    }
  });

  it('should fail for short name', () => {
      const invalidData = {
          name: 'Ab',
          price: 10,
          status: ExampleStatus.Draft
      };
      const result = ExampleSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
  });

    it('should fail for negative price', () => {
      const invalidData = {
          name: 'Valid Name',
          price: -10,
          status: ExampleStatus.Draft
      };
      const result = ExampleSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
  });
});
