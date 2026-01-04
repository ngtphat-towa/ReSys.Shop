/**
 * Pagination metadata returned by the server for list-based responses.
 */
export interface PaginationMeta {
    /** The current page number (starting from 1). */
    page: number;
    /** Number of items requested per page. */
    page_size: number;
    /** Total number of items available across all pages. */
    total_count: number;
    /** Total number of pages calculated from total_count and page_size. */
    total_pages: number;
    /** Flag indicating if there is a next page. */
    has_next_page: boolean;
    /** Flag indicating if there is a previous page. */
    has_previous_page: boolean;
}

/**
 * Standard server response envelope (Problem Details / RFC 7807 compliant).
 * Matches the backend ReSys.Core.Common.Models.ApiResponse<T> (snake_case).
 */
export interface ApiResponse<T> {
    /** The actual payload of the response. */
    data: T;
    /** Optional pagination details (only present for list endpoints). */
    meta?: PaginationMeta;
    /** Dictionary of validation errors (Key: Field name, Value: Array of error messages). */
    errors?: Record<string, string[]>;
    /** Internal error code for specific business logic failures. */
    error_code?: string;
    /** HTTP Status code. */
    status: number;
    /** Short, human-readable summary of the response/error. */
    title: string;
    /** Detailed explanation of what went wrong. */
    detail?: string;
    /** URI reference that identifies the specific occurrence of the problem. */
    type?: string;
}

/**
 * Optimized Result pattern for the frontend.
 * The Axios interceptor unwraps the server's ApiResponse into this structure.
 * 
 * @example
 * const result = await getItems();
 * if (result.success) {
 *   console.log(result.data); // T
 *   console.log(result.meta); // PaginationMeta | undefined
 * } else {
 *   console.error(result.error.title); // Partial<ApiResponse>
 * }
 */
export type ApiResult<T> = 
    | { data: T; meta?: PaginationMeta; success: true; error?: never }
    | { data: null; success: false; error: Partial<ApiResponse<any>> };