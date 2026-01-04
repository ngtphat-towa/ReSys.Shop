import axios, { type AxiosInstance, type AxiosError } from 'axios';
import { ref } from 'vue';
import type { ApiResponse } from '@/features/examples/example.types';

// Simple Event Bus for Toasts
export const toastBus = ref<{ severity: 'success' | 'info' | 'warn' | 'error'; summary: string; detail: string; life?: number } | null>(null);

export const showToast = (severity: 'success' | 'info' | 'warn' | 'error', summary: string, detail: string, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
};

const apiClient: AxiosInstance = axios.create({
    baseURL: '/api', // Vite proxy should handle this
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.response.use(
    (response) => {
        // Automatically toast on successful POST/PUT/DELETE
        if (response.config.method !== 'get' && response.status >= 200 && response.status <= 299) {
            showToast('success', 'Success', 'Action completed successfully');
        }
        return response;
    },
    (error: AxiosError) => {
        let summary = 'Error';
        let detail = 'An unexpected error occurred.';

        if (error.response) {
            const data = error.response.data as ApiResponse<any>;
            summary = data?.title || `Error ${error.response.status}`;

            if (data?.errors) {
                // Record<string, string[]> - Extract and join all validation messages
                const messages = Object.values(data.errors).flat();
                detail = messages.length > 0 ? messages.join('. ') : 'Validation failed.';
            } else {
                detail = data?.detail || error.response.statusText || 'Server error occurred.';
            }
            
            // Handle common status codes if summary/detail not provided by API
            if (error.response.status === 401 && !data?.detail) detail = 'Unauthorized. Please log in.';
            if (error.response.status === 403 && !data?.detail) detail = 'Forbidden. You do not have permission.';
        } else if (error.request) {
            detail = 'Network Error. Please check your connection.';
        }

        showToast('error', summary, detail);
        return Promise.reject(error);
    }
);

export default apiClient;