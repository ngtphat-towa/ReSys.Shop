import type { ApiResult, ApiResponse } from '@/shared/api/api.types'
import { parseApiError } from '@/shared/api/api.utils'
import { useToast } from './toast.use'

/**
 * Composable for handling API results and errors in forms and components.
 */
export function useApiErrorHandler() {
  const { showToast } = useToast()

  /**
   * Maps API validation errors to VeeValidate form errors.
   */
  const handleFormErrors = (
    error: unknown,
    setErrors: ((errors: Record<string, string | undefined>) => void) | undefined,
    fieldNames: string[],
    locales?: { errorTitle?: string; genericError?: string },
  ) => {
    if (!error) return
    const apiError = parseApiError(error)
    console.log('[API Trace] Handler received parsed error:', apiError);

    // 1. Handle Validation Errors (If 'errors' dictionary is present)
    if (apiError.errors && Object.keys(apiError.errors).length > 0) {
      console.log('[API Trace] Validation error dictionary detected.');
      const formErrors: Record<string, string> = {}
      const unmappedMessages: string[] = []

      Object.entries(apiError.errors).forEach(([key, messages]) => {
        const normalizedKey = key.toLowerCase()
        const messagesArray = messages as string[]

        const field = fieldNames.find((f) => {
          const lowerF = f.toLowerCase()
          return normalizedKey === lowerF || normalizedKey.endsWith(`.${lowerF}`)
        })

        if (field && setErrors) {
          formErrors[field] = messagesArray[0] || 'Invalid value'
        } else {
          unmappedMessages.push(...messagesArray)
        }
      })

      if (setErrors) {
        console.log('[API Trace] Mapping errors to fields:', formErrors);
        setErrors(formErrors)
      }

      // Use server detail or specific unmapped messages
      const toastDetail = apiError.detail || (unmappedMessages.length > 0 ? unmappedMessages.join('. ') : (locales?.genericError || 'Validation failed'));

      const baseTitle = apiError.title || locales?.errorTitle || 'Validation Error';
      const toastTitle = apiError.error_code ? `${baseTitle} (${apiError.error_code})` : baseTitle;

      showToast('warn', toastTitle, toastDetail);
    } else {
      // 2. Handle Global Errors (409 Conflict, 404 Not Found, 500 Server Error, etc.)
      // Follows RFC 7807 Problem Details structure (title, detail, status).
      const status = apiError.status ?? 500;
      const isClientError = status < 500;
      const severity = isClientError ? 'warn' : 'error';

      // Use Server info if available, otherwise use custom locales, otherwise system defaults
      const baseTitle = apiError.title || locales?.errorTitle || (isClientError ? 'Request Error' : 'Server Error');
      const toastTitle = apiError.error_code ? `${baseTitle} (${apiError.error_code})` : baseTitle;
      const toastDetail = apiError.detail || locales?.genericError || 'An unexpected error occurred.';

      console.log(`[API Trace] Showing global toast. Severity: ${severity}, Title: ${toastTitle}, Detail: ${toastDetail}`);
      showToast(severity, toastTitle, toastDetail);
    }
  }

  /**
   * High-level handler for ApiResult.
   * Handles success/error toasts and form error mapping.
   */
  const handleApiResult = <T>(
    result: ApiResult<T>,
    options?: {
      setErrors?: (errors: Record<string, string | undefined>) => void
      fieldNames?: string[]
      successMessage?: string
      successTitle?: string
      errorTitle?: string
      genericError?: string
    },
  ) => {
    if (result.success) {
      if (options?.successMessage) {
        showToast('success', options.successTitle || 'Success', options.successMessage)
      }
      return true
    }

    handleFormErrors(result.error, options?.setErrors, options?.fieldNames || [], {
      errorTitle: options?.errorTitle,
      genericError: options?.genericError,
    })
    return false
  }

  return {
    handleFormErrors,
    handleApiResult,
  }
}