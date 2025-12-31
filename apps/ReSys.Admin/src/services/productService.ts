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

export const createProduct = async (product: Partial<Product>): Promise<Product> => {
    const response = await apiClient.post<Product>('/products', product);
    return response.data;
};

export const updateProduct = async (id: string, product: Partial<Product>): Promise<Product> => {
    const response = await apiClient.put<Product>(`/products/${id}`, product);
    return response.data;
};

export const updateProductImage = async (id: string, file: File): Promise<Product> => {
    const formData = new FormData();
    formData.append('image', file);
    const response = await apiClient.post<Product>(`/products/${id}/image`, formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
    return response.data;
};

export const deleteProduct = async (id: string): Promise<void> => {
    await apiClient.delete(`/products/${id}`);
};
