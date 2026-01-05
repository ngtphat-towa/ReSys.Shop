import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useApiErrorHandler } from './api-error-handler.use';
import type { ApiResult } from '@/shared/api/api.types';

const mockShowToast = vi.fn();
vi.mock('./use-toast', () => ({
  useToast: () => ({
    showToast: mockShowToast
  })
}));

describe('useApiErrorHandler - Edge Cases', () => {
  beforeEach(() => {
    mockShowToast.mockClear();
  });

  it('should handle undefined error gracefully', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const setErrors = vi.fn();
    handleFormErrors(undefined, setErrors, []);
    expect(setErrors).not.toHaveBeenCalled();
  });

  it('should toast for unmapped validation errors', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const setErrors = vi.fn();
    
    const apiError = {
      status: 400,
      title: 'Validation Error',
      errors: { 'request.secret_field': ['Internal error'] }
    };

    handleFormErrors(apiError, setErrors, ['name', 'email']);

    // Should call setErrors with empty object because field didn't match
    expect(setErrors).toHaveBeenCalledWith({});
    // Should toast the unmapped message
    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Validation Error', 'Internal error');
  });

  it('should use custom locales for toasts', () => {
    const { handleApiResult } = useApiErrorHandler();
    const result: ApiResult<unknown> = {
      success: false,
      data: null,
      error: { status: 500, title: 'Server Error', detail: 'Actual error' }
    };

    handleApiResult(result, { 
      errorTitle: 'Custom Title', 
      genericError: 'Custom Detail' 
    });

    // The composable uses the locales if provided
    expect(mockShowToast).toHaveBeenCalledWith('error', 'Custom Title', 'Custom Detail');
  });

  it('should handle multiple messages for the same field by taking the first one', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const setErrors = vi.fn();
    
    const apiError = {
      status: 400,
      errors: { 'name': ['Too short', 'No numbers'] }
    };

    handleFormErrors(apiError, setErrors, ['name']);
    expect(setErrors).toHaveBeenCalledWith({ name: 'Too short' });
  });

  it('should handle handleApiResult without options', () => {
    const { handleApiResult } = useApiErrorHandler();
    const result: ApiResult<unknown> = { success: true, data: {} };
    
    // Should not crash and should return true
    expect(handleApiResult(result)).toBe(true);
    expect(mockShowToast).not.toHaveBeenCalled();
  });

  it('should normalize nested field names correctly', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const setErrors = vi.fn();
    
    const apiError = {
        status: 400,
        errors: { 'order.customer.first_name': ['Required'] }
    };

    handleFormErrors(apiError, setErrors, ['first_name']);
    expect(setErrors).toHaveBeenCalledWith({ first_name: 'Required' });
  });
});
