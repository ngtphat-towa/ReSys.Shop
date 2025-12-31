import axios, { type AxiosInstance, type AxiosError } from 'axios';
import { ref } from 'vue';

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
        return response;
    },
    (error: AxiosError) => {
        let message = 'An unexpected error occurred.';
        if (error.response) {
            switch (error.response.status) {
                case 400:
                    message = 'Bad Request.';
                    break;
                case 404:
                    message = 'Resource not found.';
                    break;
                case 500:
                    message = 'Server Error.';
                    break;
                default:
                    message = `Error: ${error.response.statusText}`;
            }
        } else if (error.request) {
            message = 'Network Error.';
        }

        showToast('error', 'Error', message);
        return Promise.reject(error);
    }
);

export default apiClient;
