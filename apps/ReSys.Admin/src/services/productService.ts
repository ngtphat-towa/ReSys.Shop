import apiClient from './apiClient';
import type { ApiResponse } from '@/types/api';
import type { 
    ProductListItem, 
    ProductDetail, 
    CreateProductRequest, 
    UpdateProductRequest, 
    ProductQuery 
} from '@/types/product';

export const getProducts = async (query?: ProductQuery): Promise<ApiResponse<ProductListItem[]>> => {
    const response = await apiClient.get<ApiResponse<ProductListItem[]>>('/products', { params: query });
    return response.data;
};

export const getProductById = async (id: string): Promise<ApiResponse<ProductDetail>> => {
    const response = await apiClient.get<ApiResponse<ProductDetail>>(`/products/${id}`);
    return response.data;
};

export const createProduct = async (request: CreateProductRequest): Promise<ApiResponse<ProductDetail>> => {
    const response = await apiClient.post<ApiResponse<ProductDetail>>('/products', request);
    return response.data;
};

export const updateProduct = async (id: string, request: UpdateProductRequest): Promise<ApiResponse<ProductDetail>> => {
    const response = await apiClient.put<ApiResponse<ProductDetail>>(`/products/${id}`, request);
    return response.data;
};

export const updateProductImage = async (id: string, file: File): Promise<ApiResponse<ProductDetail>> => {
    const formData = new FormData();
    formData.append('image', file);
    const response = await apiClient.post<ApiResponse<ProductDetail>>(`/products/${id}/image`, formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
    return response.data;
};

export const deleteProduct = async (id: string): Promise<void> => {
    await apiClient.delete(`/products/${id}`);
};

export const getSimilarProducts = async (id: string): Promise<ApiResponse<ProductListItem[]>> => {
    const response = await apiClient.get<ApiResponse<ProductListItem[]>>(`/products/${id}/similar`);
    return response.data;
};