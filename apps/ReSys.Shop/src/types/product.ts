export interface ProductBase {
    name: string;
    description: string;
    price: number;
    image_url: string;
}

export interface ProductListItem {
    id: string;
    name: string;
    price: number;
    image_url: string;
    description: string;
}

export interface ProductDetail extends ProductBase {
    id: string;
    created_at: string;
}

export interface ProductQuery {
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
