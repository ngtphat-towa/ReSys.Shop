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

export interface CreateExampleRequest {
    name: string;
    description: string;
    price: number;
    image_url?: string;
}

export interface UpdateExampleRequest {
    name: string;
    description: string;
    price: number;
    image_url?: string;
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
