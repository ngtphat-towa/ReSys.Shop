import type { ApiResponse } from './api.types'

/**
 * Standardizes an error (Axios or otherwise) into a consistent ApiResponse shape.
 * This function is idempotent and robust against various server response shapes.
 */
export function parseApiError(error: unknown): Partial<ApiResponse<unknown>> {
  console.log('[API Trace] Raw error input:', error);

  // 1. Idempotency & Safety Check
  if (!error || typeof error !== 'object') {
    return {
      status: 500,
      title: 'Connection Error',
      detail: 'An unexpected error occurred.',
    };
  }

  // 2. Handle Axios Error (Highest Priority)
  const axiosError = error as any;
  if (axiosError.isAxiosError || axiosError.response || axiosError.request) {
    const apiData = axiosError.response?.data;
    console.log('[API Trace] Axios error detected. Body data:', apiData);

    if (apiData && typeof apiData === 'object') {
      // Handle both snake_case and PascalCase from various backend setups
      const status = apiData.status ?? apiData.Status ?? axiosError.response?.status;
      const title = apiData.title ?? apiData.Title;
      const detail = apiData.detail ?? apiData.Detail;
      const errors = apiData.errors ?? apiData.Errors;
      const errorCode = apiData.error_code ?? apiData.ErrorCode ?? apiData.errorCode;

      const result = {
        status: status ?? 500,
        title: title ?? (status && status >= 500 ? 'Server Error' : 'Request Error'),
        detail: detail,
        errors: errors,
        error_code: errorCode,
      };
      console.log('[API Trace] Successfully parsed from Axios response:', result);
      return result;
    }

    if (axiosError.request && !axiosError.response) {
      return {
        status: 500,
        title: 'Connection Error',
        detail: axiosError.message || 'Network Error. Please check your internet connection.',
      };
    }
  }

  // 3. Handle already parsed/standardized objects (Idempotency)
  const e = error as any;
  if (e.status !== undefined && (e.title !== undefined || e.detail !== undefined || e.errors !== undefined)) {
    console.log('[API Trace] Error is already parsed:', e);
    return {
      status: e.status,
      title: e.title,
      detail: e.detail,
      errors: e.errors,
      error_code: e.error_code,
    };
  }

  // 4. Final generic fallback
  return {
    status: 500,
    // Do not provide defaults here so the UI can decide on fallbacks
  };
}
