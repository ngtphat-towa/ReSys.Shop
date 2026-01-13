import axios, { type AxiosInstance, type AxiosError, type AxiosResponse } from 'axios'
import type { ApiResponse, ApiResult } from './api.types'
import { parseApiError } from './api.utils'

/**
 * Configured Axios instance for the Shop application.
 * Handles automatic unwrapping of the server envelope.
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: '/api', // Vite proxy should handle this
  headers: {
    'Content-Type': 'application/json',
  },
  paramsSerializer: {
    indexes: null, // No brackets for array parameters
  },
});

apiClient.interceptors.response.use(
  (response) => {
    /**
     * SUCCESS HANDLER
     * Unwrap the server's ApiResponse envelope to simplify data access in the UI.
     */
    const apiResponse = response.data as ApiResponse<unknown>;

    const result: ApiResult<unknown> = {
        data: apiResponse.data,
        meta: apiResponse.meta,
        success: true
    };

    return result as unknown as AxiosResponse;
  },
  (error: AxiosError) => {
    const apiError = parseApiError(error)

    // 1. Global Interceptor Responsibilities
    // Here we could handle 401 (redirect to login) or 403 globally
    if (apiError.status === 401) {
      console.warn('Session expired. Redirecting to login...')
    }

    // 2. Standardized Result Pattern return
    return Promise.resolve({
      data: null,
      success: false,
      error: apiError,
    })
  }
);

export default apiClient;
