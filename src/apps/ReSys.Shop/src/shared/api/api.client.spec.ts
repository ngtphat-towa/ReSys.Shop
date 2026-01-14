import { describe, it, expect, vi, beforeEach } from 'vitest';
import apiClient from './api.client';
import { parseApiError } from './api.utils';
import type { AxiosResponse } from 'axios';

// Mock parseApiError
vi.mock('./api.utils', () => ({
  parseApiError: vi.fn((err) => ({
    status: err.response?.status || 500,
    title: 'Mock Error',
    detail: 'Mock Detail'
  }))
}));

describe('apiClient', () => {
  let successInterceptor: (response: AxiosResponse) => unknown;
  let errorInterceptor: (error: unknown) => Promise<unknown>;

  beforeEach(() => {
    vi.clearAllMocks();
    
    // Find the interceptors from the apiClient instance
    const responseInterceptor = (apiClient.interceptors.response as unknown as { 
      handlers: Array<{ fulfilled: (res: AxiosResponse) => unknown, rejected: (err: unknown) => Promise<unknown> }> 
    }).handlers[0];

    if (!responseInterceptor) {
        throw new Error('Response interceptor not found');
    }

    successInterceptor = responseInterceptor.fulfilled;
    errorInterceptor = responseInterceptor.rejected;
  });

  it('should unwrap successful response data', () => {
    const mockResponse = {
      data: {
        data: { id: 1, name: 'Shop Item' },
        status: 200,
        title: 'Success'
      }
    } as AxiosResponse;

    const result = successInterceptor(mockResponse);

    expect(result).toEqual({
      data: { id: 1, name: 'Shop Item' },
      meta: undefined,
      success: true
    });
  });

  it('should parse and format error response', async () => {
    const mockError = {
      isAxiosError: true,
      response: {
        status: 404,
        data: { title: 'Not Found' }
      }
    };

    const result = await errorInterceptor(mockError);

    expect(parseApiError).toHaveBeenCalledWith(mockError);
    expect(result).toEqual({
      data: null,
      success: false,
      error: {
        status: 404,
        title: 'Mock Error',
        detail: 'Mock Detail'
      }
    });
  });
});
