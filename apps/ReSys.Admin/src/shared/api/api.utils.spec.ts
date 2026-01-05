import { describe, it, expect } from 'vitest';
import { parseApiError } from './api.utils';

describe('parseApiError - Edge Cases', () => {
  it('should handle null or undefined input gracefully', () => {
    expect(parseApiError(null as unknown)).toEqual({
      status: 500,
      title: 'Connection Error',
      detail: 'An unexpected error occurred.'
    });
  });

  it('should handle non-object inputs', () => {
    expect(parseApiError('Something went wrong')).toEqual({
      status: 500,
      title: 'Connection Error',
      detail: 'An unexpected error occurred.'
    });
  });

  it('should extract status from response if missing in apiError body', () => {
    const error = {
      response: {
        status: 403,
        data: { title: 'Forbidden' } // No status field in body
      }
    };
    const result = parseApiError(error);
    expect(result.status).toBe(403);
    expect(result.title).toBe('Forbidden');
  });

  it('should handle Axios timeouts (ECONNABORTED)', () => {
    const timeoutError = {
      code: 'ECONNABORTED',
      message: 'timeout of 1000ms exceeded',
      request: {}
    };
    const result = parseApiError(timeoutError);
    expect(result.title).toBe('Connection Error');
    expect(result.detail).toBe('timeout of 1000ms exceeded');
  });

  it('should handle empty validation errors object', () => {
    const error = {
      response: {
        status: 400,
        data: { title: 'Bad Request', errors: {} }
      }
    };
    const result = parseApiError(error);
    expect(result.errors).toEqual({});
  });

  it('should prioritize apiError.status over axios response status', () => {
    const error = {
      response: {
        status: 400,
        data: { status: 422, title: 'Unprocessable' }
      }
    };
    const result = parseApiError(error);
    expect(result.status).toBe(422);
  });
});