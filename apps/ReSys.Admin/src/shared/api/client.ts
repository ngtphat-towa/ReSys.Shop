import axios, { type AxiosInstance, type AxiosError } from 'axios';
import { ref } from 'vue';
import type { ApiResponse, ApiResult } from './types';

/**
 * Global Event Bus for Notifications.
 * Components can watch this ref to display success/error toasts globally.
 */
export const toastBus = ref<{ severity: 'success' | 'info' | 'warn' | 'error'; summary: string; detail: string; life?: number } | null>(null);

/**
 * Utility to trigger a global toast notification.
 */
export const showToast = (severity: 'success' | 'info' | 'warn' | 'error', summary: string, detail: string, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
};

/**
 * Configured Axios instance with ReSys-specific interceptors.
 * It handles automatic unwrapping of the server envelope and global error notification.
 */
const apiClient: AxiosInstance = axios.create({
    baseURL: '/api', // Proxied via Vite config to the backend
    headers: {
        'Content-Type': 'application/json',
    },
});

// --- RESPONSE INTERCEPTOR ---
apiClient.interceptors.response.use(
    (response) => {
        /**
         * SUCCESS HANDLER
         * Logic: 
         * 1. If it's a mutation (POST/PUT/DELETE) and successful, show a success toast.
         * 2. Unwrap the server's ApiResponse envelope to simplify data access in the UI.
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
         * Logic:
         * 1. Parse standard RFC 7807 problem details from the server.
         * 2. Handle Network vs Server vs Validation errors.
         * 3. Show global toasts for critical errors (excluding validation/conflict errors).
         */
        let summary = 'Error';
        let detail = 'An unexpected error occurred.';
        let apiError: ApiResponse<any> | null = null;

        if (error.response) {
            // The server responded with a non-2xx status code
            apiError = error.response.data as ApiResponse<any>;
            summary = apiError?.title || `Error ${error.response.status}`;

            if (apiError?.errors) {
                // Flatten validation errors for the toast message
                const messages = Object.values(apiError.errors).flat();
                detail = messages.length > 0 ? messages.join('. ') : 'Validation failed.';
            } else {
                detail = apiError?.detail || error.response.statusText || 'Server error occurred.';
            }
        } else if (error.request) {
            // The request was made but no response was received (e.g., server down)
            detail = 'Network Error. Please check your connection.';
        }

        // Only show toast for non-400/409 errors (those are usually handled by form validation in the UI)
        if (error.response?.status !== 400 && error.response?.status !== 409) {
            showToast('error', summary, detail);
        }

        // Return a failed Result object instead of throwing, allowing for cleaner async/await logic
        return Promise.resolve({
            data: null,
            success: false,
            error: apiError || { status: 500, title: summary, detail }
        });
    }
);

export default apiClient;
