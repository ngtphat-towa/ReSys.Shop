import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useApiErrorHandler } from './api-error-handler.use';
import type { ApiResult } from '@/shared/api/api.types';

const mockShowToast = vi.fn();
vi.mock('./toast.use', () => ({
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

  it('should include error_code in the toast title', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const apiError = {
      status: 409,
      title: 'Conflict',
      detail: 'Already exists',
      error_code: 'DuplicateName'
    };

    handleFormErrors(apiError, undefined, []);
    expect(mockShowToast).toHaveBeenCalledWith(
      'warn', 
      'Conflict (DuplicateName)', 
      'Already exists'
    );
  });

  it('should prioritize apiError.detail over genericError locale', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const apiError = {
      status: 500,
      title: 'Error',
      detail: 'Specific server error message'
    };

    handleFormErrors(apiError, undefined, [], { genericError: 'Generic Locale Error' });
    expect(mockShowToast).toHaveBeenCalledWith('error', 'Error', 'Specific server error message');
  });

  it('should prioritize apiError.title over errorTitle locale', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const apiError = {
      status: 409,
      title: 'Conflict',
      detail: 'Detail'
    };

    handleFormErrors(apiError, undefined, [], { errorTitle: 'Generic Error' });
    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Conflict', 'Detail');
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

  it('should prioritize apiError over custom locales', () => {
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

    // apiError.title ('Server Error') and apiError.detail ('Actual error') should win
    expect(mockShowToast).toHaveBeenCalledWith('error', 'Server Error', 'Actual error');
  });

  it('should use custom locales as fallback when apiError fields are missing', () => {
    const { handleApiResult } = useApiErrorHandler();
    const result: ApiResult<unknown> = {
      success: false,
      data: null,
      error: {} as Record<string, unknown>
    };

    handleApiResult(result, { 
      errorTitle: 'Fallback Title', 
      genericError: 'Fallback Detail' 
    });

    expect(mockShowToast).toHaveBeenCalledWith('error', 'Fallback Title', 'Fallback Detail');
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
