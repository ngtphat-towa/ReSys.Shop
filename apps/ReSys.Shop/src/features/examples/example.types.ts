import type { ApiResponse } from '@/shared/api/api.types';
import type { ExampleInput } from './example.schema';

/**
 * Shared types for the Example feature in Shop
 */
export interface ExampleListItem extends ExampleInput {
    id: string;
}

export interface ExampleDetail extends ExampleInput {
    id: string;
    created_at: string;
}

export interface ExampleQuery {
    search?: string;
    name?: string;
    min_price?: number;
    max_price?: number;
    status?: number[];
    created_from?: string;
    created_to?: string;
    sort_by?: string;
    is_descending?: boolean;
    page?: number;
    page_size?: number;
}

export type { ApiResponse, ExampleInput };
