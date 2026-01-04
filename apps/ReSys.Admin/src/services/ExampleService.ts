import apiClient from './apiClient';
import type { ApiResponse } from '@/types/api';
import type {
    ExampleListItem,
    ExampleDetail,
    CreateExampleRequest,
    UpdateExampleRequest,
    ExampleQuery
} from '@/types/Example';
export const getExamples = async (query?: ExampleQuery): Promise<ApiResponse<ExampleListItem[]>> => {    
    const response = await apiClient.get<ApiResponse<ExampleListItem[]>>('/examples', { params: query });
    return response.data;
};

export const getExampleById = async (id: string): Promise<ApiResponse<ExampleDetail>> => {
    const response = await apiClient.get<ApiResponse<ExampleDetail>>(`/examples/${id}`);
    return response.data;
};

export const createExample = async (request: CreateExampleRequest): Promise<ApiResponse<ExampleDetail>> => {
    const response = await apiClient.post<ApiResponse<ExampleDetail>>('/examples', request);
    return response.data;
};

export const updateExample = async (id: string, request: UpdateExampleRequest): Promise<ApiResponse<ExampleDetail>> => {
    const response = await apiClient.put<ApiResponse<ExampleDetail>>(`/examples/${id}`, request);        
    return response.data;
};

export const updateExampleImage = async (id: string, file: File): Promise<ApiResponse<ExampleDetail>> => {
    const formData = new FormData();
    formData.append('image', file);
    const response = await apiClient.post<ApiResponse<ExampleDetail>>(`/examples/${id}/image`, formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
    return response.data;
};

export const deleteExample = async (id: string): Promise<void> => {
    await apiClient.delete(`/examples/${id}`);
};

export const getSimilarExamples = async (id: string): Promise<ApiResponse<ExampleListItem[]>> => {       
    const response = await apiClient.get<ApiResponse<ExampleListItem[]>>(`/examples/${id}/similar`);     
    return response.data;
};