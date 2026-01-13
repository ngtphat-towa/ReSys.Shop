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
    successInterceptor = responseInterceptor.fulfilled;
    errorInterceptor = responseInterceptor.rejected;
  });

  it('should unwrap successful response data', () => {
    const mockResponse = {
      data: {
        data: { id: 1, name: 'Test' },
        meta: { total_count: 1 },
        status: 200,
        title: 'Success'
      }
    };

    const result = successInterceptor(mockResponse);

    expect(result).toEqual({
      data: { id: 1, name: 'Test' },
      meta: { total_count: 1 },
      success: true
    });
  });

  it('should parse and format error response', async () => {
    const mockError = {
      isAxiosError: true,
      response: {
        status: 400,
        data: { title: 'Bad Request' }
      }
    };

    const result = await errorInterceptor(mockError);

    expect(parseApiError).toHaveBeenCalledWith(mockError);
    expect(result).toEqual({
      data: null,
      success: false,
      error: {
        status: 400,
        title: 'Mock Error',
        detail: 'Mock Detail'
      }
    });
  });

  it('should log warning on 401 status', async () => {
    const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
    
    // Force parseApiError to return 401
    vi.mocked(parseApiError).mockReturnValueOnce({ status: 401 });

    const mockError = { response: { status: 401 } };
    await errorInterceptor(mockError);

    expect(consoleSpy).toHaveBeenCalledWith(expect.stringContaining('Session expired'));
    consoleSpy.mockRestore();
  });
});