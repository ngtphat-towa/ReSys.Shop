import apiClient from './apiClient';

export interface Product {
    id: string;
    name: string;
    description: string;
    price: number;
    image_url: string;
    created_at: string;
}

export const getProducts = async (): Promise<Product[]> => {
    const response = await apiClient.get<Product[]>('/products');
    return response.data;
};

export const getProductById = async (id: string): Promise<Product> => {
    const response = await apiClient.get<Product>(`/products/${id}`);
    return response.data;
};
