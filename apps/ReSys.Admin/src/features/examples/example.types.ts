import type { ApiResponse } from '@/shared/api/api.types';

/**
 * Mirror of the Backend 'ExampleInput'
 * Use this for shared form logic
 */
export interface ExampleInput {
    name: string;
    description: string;
    price: number;
    image_url?: string;
}

export interface ExampleListItem extends ExampleInput {
    id: string;
}

export interface ExampleDetail extends ExampleInput {
    id: string;
    created_at: string;
}

/**
 * Semantic requests - keeps your code descriptive
 */
export interface CreateExampleRequest extends ExampleInput {}

export interface UpdateExampleRequest extends ExampleInput {}

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

// Re-export ApiResponse for convenience if features need it directly
export type { ApiResponse };