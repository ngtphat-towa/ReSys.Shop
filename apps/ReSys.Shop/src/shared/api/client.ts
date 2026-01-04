import axios, { type AxiosInstance, type AxiosError } from 'axios';
import { ref } from 'vue';
import type { ApiResponse, ApiResult } from './types';

/**
 * Global Event Bus for Notifications.
 * Used for displaying success/error messages across the Shop application.
 */
export const toastBus = ref<{ severity: 'success' | 'info' | 'warn' | 'error'; summary: string; detail: string; life?: number } | null>(null);

/**
 * Utility to trigger a global toast notification.
 */
export const showToast = (severity: 'success' | 'info' | 'warn' | 'error', summary: string, detail: string, life = 3000) => {
  toastBus.value = { severity, summary, detail, life };
};

/**
 * Configured Axios instance for the Shop application.
 * Handles automatic unwrapping of the server envelope.
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: '/api', // Vite proxy should handle this
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.response.use(
  (response) => {
    /**
     * SUCCESS HANDLER
     * Automatically shows success notifications for mutations (POST, PUT, DELETE).
     * Unwraps the server envelope to provide clean data access.
     */
    if (response.config.method !== 'get' && response.status >= 200 && response.status <= 299) {
      showToast('success', 'Success', 'Action completed successfully');
    }
    
    const apiResponse = response.data as ApiResponse<any>;

    return {
        data: apiResponse.data,
        meta: apiResponse.meta,
        success: true
    } as any;
  },
  (error: AxiosError) => {
    const apiError = error.response?.data as ApiResponse<any> | undefined;
    
    // 1. Extract meaningful error messages
    const summary = apiError?.title || (error.response ? `Error ${error.response.status}` : 'Connection Error');
    let detail = apiError?.detail || error.message || 'An unexpected error occurred.';

    // 2. Network Error check
    if (!error.response && error.request) {
      detail = 'Network Error. Please check your internet connection.';
    }

    // 3. SMART TOAST LOGIC
    const hasFieldErrors = !!(apiError?.errors && Object.keys(apiError.errors).length > 0);

    if (!hasFieldErrors) {
      showToast(error.response && error.response.status < 500 ? 'warn' : 'error', summary, detail);
    }

    // 4. Standardized Result Pattern return
    return Promise.resolve({
      data: null,
      success: false,
      error: apiError || { 
        status: error.response?.status || 500, 
        title: summary, 
        detail: detail 
      }
    });
  }
);

export default apiClient;