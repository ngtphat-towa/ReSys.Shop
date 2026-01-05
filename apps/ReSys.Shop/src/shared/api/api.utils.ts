import { type AxiosError } from 'axios'
import type { ApiResponse } from './api.types'

/**
 * Standardizes an error (Axios or otherwise) into a consistent ApiResponse shape.
 */
export function parseApiError(error: unknown): Partial<ApiResponse<unknown>> {
    // 1. Idempotency & Safety Check
    if (!error || typeof error !== 'object') {
        return {
            status: 500,
            title: 'Connection Error',
            detail: 'An unexpected error occurred.'
        };
    }

    // If it already looks like our standardized ApiResponse, return it.
    // We check for 'status' OR 'errors' to be safe.
    if ('status' in error || 'errors' in error) {
        return error as Partial<ApiResponse<unknown>>;
    }

    const axiosError = error as AxiosError;
    const apiError = axiosError.response?.data as ApiResponse<unknown> | undefined;

    if (apiError && typeof apiError === 'object') {
        return {
            ...apiError,
            status: apiError.status || axiosError.response?.status || 500
        };
    }

    // 2. Fallback for network errors, timeouts, or non-standard responses
    const status = axiosError.response?.status || 500;
    const title = axiosError.response ? `Error ${status}` : 'Connection Error';
    
    let detail = axiosError.message || 'An unexpected error occurred.';
    
    if (!axiosError.response && !axiosError.request) {
        detail = 'An unexpected error occurred.';
    } else if (!axiosError.response && axiosError.request && !axiosError.code) {
        detail = 'Network Error. Please check your internet connection.';
    }

    return {
        status,
        title,
        detail
    };
}
