import { describe, it, expect } from 'vitest';
import { parseApiError } from './api.utils';

describe('parseApiError', () => {
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

  it('should handle PascalCase properties from backend', () => {
    const error = {
      response: {
        data: { Status: 409, Title: 'Conflict', Detail: 'Already exists', ErrorCode: 'Duplicate' }
      }
    };
    const result = parseApiError(error);
    expect(result.status).toBe(409);
    expect(result.title).toBe('Conflict');
    expect(result.detail).toBe('Already exists');
    expect(result.error_code).toBe('Duplicate');
  });

  it('should handle snake_case error_code', () => {
    const error = {
      response: {
        data: { error_code: 'some_code' }
      }
    };
    const result = parseApiError(error);
    expect(result.error_code).toBe('some_code');
  });

  it('should extract status from response if missing in apiError body', () => {
    const error = {
      response: {
        status: 403,
        data: { title: 'Forbidden' }
      }
    };
    const result = parseApiError(error);
    expect(result.status).toBe(403);
    expect(result.title).toBe('Forbidden');
  });

  it('should be idempotent and return already parsed errors', () => {
    const parsedError = {
      status: 400,
      title: 'Already Parsed',
      detail: 'Details here',
      error_code: 'CODE'
    };
    const result = parseApiError(parsedError);
    expect(result).toEqual(parsedError);
  });

  it('should handle Axios timeouts', () => {
    const timeoutError = {
      code: 'ECONNABORTED',
      message: 'timeout of 1000ms exceeded',
      request: {}
    };
    const result = parseApiError(timeoutError);
    expect(result.title).toBe('Connection Error');
    expect(result.detail).toBe('timeout of 1000ms exceeded');
  });
});
