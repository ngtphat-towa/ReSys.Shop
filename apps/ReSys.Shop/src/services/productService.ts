import apiClient from './apiClient';
import type { ApiResponse } from '@/types/api';
import type { 
    ProductListItem, 
    ProductDetail, 
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

export const getSimilarProducts = async (id: string): Promise<ApiResponse<ProductListItem[]>> => {
    const response = await apiClient.get<ApiResponse<ProductListItem[]>>(`/products/${id}/similar`);
    return response.data;
};