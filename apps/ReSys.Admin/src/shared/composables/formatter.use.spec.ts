import { describe, it, expect } from 'vitest';
import { useFormatter } from './formatter.use';

describe('useFormatter', () => {
  const { formatCurrency, truncate } = useFormatter();

  describe('formatCurrency', () => {
    it('should format numbers as USD', () => {
      // Use a regex to match the currency format, handling different whitespace characters (like non-breaking spaces)
      expect(formatCurrency(1234.56)).toMatch(/\$1,234\.56/);
      expect(formatCurrency(0)).toMatch(/\$0\.00/);
    });

    it('should return empty string for null or undefined', () => {
      expect(formatCurrency(null)).toBe('');
      expect(formatCurrency(undefined)).toBe('');
    });
  });

  describe('truncate', () => {
    it('should truncate long strings', () => {
      expect(truncate('Hello World', 5)).toBe('Hello...');
    });

    it('should not truncate short strings', () => {
      expect(truncate('Hello', 10)).toBe('Hello');
    });

    it('should handle null or undefined', () => {
      expect(truncate(null, 10)).toBe('');
      expect(truncate(undefined, 5)).toBe('');
    });
  });
});
