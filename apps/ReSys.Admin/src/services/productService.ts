import apiClient from './apiClient';

export interface Product {
    id: string;
    name: string;
    description: string;
    price: number;
    imageUrl: string;
    createdAt: string;
}

export const getProducts = async (): Promise<Product[]> => {
    const response = await apiClient.get<Product[]>('/products');
    return response.data;
};

export const getProductById = async (id: string): Promise<Product> => {
    const response = await apiClient.get<Product>(`/products/${id}`);
    return response.data;
};

export const createProduct = async (formData: FormData): Promise<Product> => {
    const response = await apiClient.post<Product>('/products', formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
    return response.data;
};

export const updateProduct = async (id: string, formData: FormData): Promise<Product> => {
    const response = await apiClient.put<Product>(`/products/${id}`, formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
    return response.data;
};

export const deleteProduct = async (id: string): Promise<void> => {
    await apiClient.delete(`/products/${id}`);
};
