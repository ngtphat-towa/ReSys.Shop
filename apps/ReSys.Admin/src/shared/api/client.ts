import axios, { type AxiosInstance, type AxiosError } from 'axios';
import { ref } from 'vue';
import type { ApiResponse, ApiResult } from './types';

// Simple Event Bus for Toasts
export const toastBus = ref<{ severity: 'success' | 'info' | 'warn' | 'error'; summary: string; detail: string; life?: number } | null>(null);

export const showToast = (severity: 'success' | 'info' | 'warn' | 'error', summary: string, detail: string, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
};

const apiClient: AxiosInstance = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.response.use(
    (response) => {
        // Automatic success notification for mutations
        if (response.config.method !== 'get' && response.status >= 200 && response.status <= 299) {
            showToast('success', 'Success', 'Action completed successfully');
        }
        
        // Wrap success in Result pattern
        return {
            data: response.data,
            success: true
        } as any;
    },
    (error: AxiosError) => {
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

        // Only show toast for non-validation/conflict errors (let the form handle those)
        if (error.response?.status !== 400 && error.response?.status !== 409) {
            showToast('error', summary, detail);
        }

        // Return failure instead of throwing
        return Promise.resolve({
            data: null,
            success: false,
            error: apiError || { status: 500, title: summary, detail }
        });
    }
);

export default apiClient;
