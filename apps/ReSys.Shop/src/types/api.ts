export interface PaginationMeta {
    page: number;
    page_size: number;
    total_count: number;
    total_pages: number;
    has_next_page: boolean;
    has_previous_page: boolean;
}

export interface ApiResponse<T> {
    data: T;
    meta?: PaginationMeta;
    errors?: Record<string, string[]>;
    error_code?: string;
    status: number;
    title: string;
    detail?: string;
    type?: string;
}
