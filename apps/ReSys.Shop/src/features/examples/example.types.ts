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

export interface ExampleBase {
    name: string;
    description: string;
    price: number;
    image_url: string;
}

export interface ExampleListItem {
    id: string;
    name: string;
    description: string;
    price: number;
    image_url: string;
}

export interface ExampleDetail extends ExampleBase {
    id: string;
    created_at: string;
}

export interface ExampleQuery {
    search?: string;
    name?: string;
    min_price?: number;
    max_price?: number;
    created_from?: string;
    created_to?: string;
    sort_by?: string;
    is_descending?: boolean;
    page?: number;
    page_size?: number;
}
