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
    }
  }

  // 2. Handle Axios Error (Highest Priority)
  const axiosError = error as {
    isAxiosError?: boolean;
    response?: { data?: Record<string, unknown>; status?: number };
    request?: unknown;
    message?: string;
  };
  if (axiosError.isAxiosError || axiosError.response || axiosError.request) {
    const apiData = axiosError.response?.data;
    console.log('[API Trace] Axios error detected. Body data:', apiData);

    if (apiData && typeof apiData === 'object') {
      const data = apiData as Record<string, unknown>;
      // Handle both snake_case and PascalCase from various backend setups
      const status = (data.status ?? data.Status ?? axiosError.response?.status) as number | undefined;
      const title = (data.title ?? data.Title) as string | undefined;
      const detail = (data.detail ?? data.Detail) as string | undefined;
      const errors = (data.errors ?? data.Errors) as Record<string, string[]> | undefined;
      const errorCode = (data.error_code ?? data.ErrorCode ?? data.errorCode) as string | undefined;

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
  const e = error as Record<string, unknown>;
  if (e.status !== undefined && (e.title !== undefined || e.detail !== undefined || e.errors !== undefined)) {
    console.log('[API Trace] Error is already parsed:', e);
    return {
      status: e.status as number,
      title: e.title as string,
      detail: e.detail as string,
      errors: e.errors as Record<string, string[]>,
      error_code: e.error_code as string,
    }
  }

  // 4. Final generic fallback
  return {
    status: 500,
  }
}