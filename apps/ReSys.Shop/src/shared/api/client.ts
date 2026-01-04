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
    /**
     * ERROR HANDLER
     * Parses RFC 7807 problem details and triggers global error notifications.
     */
    let summary = 'Error';
    let detail = 'An unexpected error occurred.';
    let apiError: ApiResponse<any> | null = null;

    if (error.response) {
      apiError = error.response.data as ApiResponse<any>;
      summary = apiError?.title || `Error ${error.response.status}`;

      if (apiError?.errors) {
        const messages = Object.values(apiError.errors).flat();
        detail = messages.length > 0 ? messages.join('. ') : 'Validation failed.';
      } else {
        detail = apiError?.detail || error.response.statusText || 'Server error occurred.';
      }
    } else if (error.request) {
      detail = 'Network Error. Please check your connection.';
    }

    if (error.response?.status !== 400 && error.response?.status !== 409) {
        showToast('error', summary, detail);
    }

    return Promise.resolve({
        data: null,
        success: false,
        error: apiError || { status: 500, title: summary, detail }
    });
  }
);

export default apiClient;