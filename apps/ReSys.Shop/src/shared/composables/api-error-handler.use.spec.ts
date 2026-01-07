import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useApiErrorHandler } from './api-error-handler.use';
import type { ApiResult } from '@/shared/api/api.types';

const mockShowToast = vi.fn();
vi.mock('./toast.use', () => ({
  useToast: () => ({
    showToast: mockShowToast
  })
}));

describe('useApiErrorHandler', () => {
  beforeEach(() => {
    mockShowToast.mockClear();
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

  it('should handle validation errors and map them to fields', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const setErrors = vi.fn();
    
    const apiError = {
      status: 400,
      errors: { 'name': ['Too short'] }
    };

    handleFormErrors(apiError, setErrors, ['name']);
    expect(setErrors).toHaveBeenCalledWith({ name: 'Too short' });
  });

  it('should toast for unmapped validation errors', () => {
    const { handleFormErrors } = useApiErrorHandler();
    const apiError = {
      status: 400,
      title: 'Validation Error',
      errors: { 'secret': ['Some internal validation failed'] }
    };

    handleFormErrors(apiError, undefined, ['name']);
    expect(mockShowToast).toHaveBeenCalledWith('warn', 'Validation Error', 'Some internal validation failed');
  });

  it('should handle handleApiResult success', () => {
    const { handleApiResult } = useApiErrorHandler();
    const result: ApiResult<string> = { success: true, data: 'ok' };
    
    const handled = handleApiResult(result, { successMessage: 'Done' });
    expect(handled).toBe(true);
    expect(mockShowToast).toHaveBeenCalledWith('success', 'Success', 'Done');
  });
});