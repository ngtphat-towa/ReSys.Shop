import axios, { type AxiosInstance, type AxiosError } from 'axios';
import { ref } from 'vue';

// Simple Event Bus for Toasts
export const toastBus = ref<{ severity: 'success' | 'info' | 'warn' | 'error'; summary: string; detail: string; life?: number } | null>(null);

export const showToast = (severity: 'success' | 'info' | 'warn' | 'error', summary: string, detail: string, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
};

const apiClient: AxiosInstance = axios.create({
    baseURL: '/api', // Vite proxy should handle this, or set VITE_API_URL
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.response.use(
    (response) => {
        // Optional: Show success toast for specific methods if desired, 
        // but usually we only want to toast on explicit actions or errors.
        return response;
    },
    (error: AxiosError) => {
        let message = 'An unexpected error occurred.';
        if (error.response) {
            // Server responded with a status code out of 2xx range
            switch (error.response.status) {
                case 400:
                    message = 'Bad Request. Please check your input.';
                    break;
                case 401:
                    message = 'Unauthorized. Please log in.';
                    break;
                case 403:
                    message = 'Forbidden. You do not have permission.';
                    break;
                case 404:
                    message = 'Resource not found.';
                    break;
                case 500:
                    message = 'Server Error. Please try again later.';
                    break;
                default:
                    message = `Error: ${error.response.statusText}`;
            }
        } else if (error.request) {
            // Request made but no response received
            message = 'Network Error. Please check your connection.';
        }

        showToast('error', 'Error', message);
        return Promise.reject(error);
    }
);

export default apiClient;
