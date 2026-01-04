import type { ApiResponse } from '@/shared/api/types';
import type { ExampleInput } from './example.validator';

/**
 * Shared types for the Example feature
 */
export interface ExampleListItem extends ExampleInput {
    id: string;
}

export interface ExampleDetail extends ExampleInput {
    id: string;
    created_at: string;
}

/**
 * Semantic requests
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

export type { ApiResponse, ExampleInput };
