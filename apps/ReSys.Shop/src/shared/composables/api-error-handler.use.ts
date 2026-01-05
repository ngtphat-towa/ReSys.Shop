import type { ApiResult } from '@/shared/api/api.types'
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

    // 1. Handle Validation Errors (If errors dictionary is present)
    if (apiError.errors && Object.keys(apiError.errors).length > 0) {
      const formErrors: Record<string, string> = {}
      const unmappedMessages: string[] = []

      Object.entries(apiError.errors).forEach(([key, messages]) => {
        // Backend keys are often prefixed (e.g., 'request.name', 'order.customer.name')
        const normalizedKey = key.toLowerCase()
        const messagesArray = messages as string[]
        
        // Match if the key is exactly the field name, 
        // OR if the key ends with .field_name (e.g. 'request.name' matching 'name')
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

      if (setErrors) setErrors(formErrors)

      // Show toast for validation errors that couldn't be bound to a specific input
      if (unmappedMessages.length > 0) {
        showToast(
          'warn',
          locales?.errorTitle || apiError.title || 'Validation Error',
          unmappedMessages.join('. '),
        )
      }
    } else {
      // 2. Handle Global Errors (409, 500, etc.)
      showToast(
        apiError.status && apiError.status < 500 ? 'warn' : 'error',
        locales?.errorTitle || apiError.title || 'Error',
        locales?.genericError || apiError.detail || 'An unexpected error occurred.',
      )
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
