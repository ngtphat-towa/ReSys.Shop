import apiClient from '@/shared/api/client';
import type { ApiResult } from '@/shared/api/types';
import type { 
    ExampleListItem,
    ExampleDetail,
    CreateExampleRequest,
    UpdateExampleRequest,
    ExampleQuery
} from './example.types';

export const getExamples = async (query?: ExampleQuery): Promise<ApiResult<ExampleListItem[]>> => {    
    return await apiClient.get('/examples', { params: query });
};

export const getExampleById = async (id: string): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.get(`/examples/${id}`);
};

export const createExample = async (request: CreateExampleRequest): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.post('/examples', request);
};

export const updateExample = async (id: string, request: UpdateExampleRequest): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.put(`/examples/${id}`, request);        
};

export const updateExampleImage = async (id: string, file: File): Promise<ApiResult<ExampleDetail>> => {
    const formData = new FormData();
    formData.append('image', file);
    return await apiClient.post(`/examples/${id}/image`, formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
};

export const deleteExample = async (id: string): Promise<ApiResult<void>> => {
    return await apiClient.delete(`/examples/${id}`);
};

export const getSimilarExamples = async (id: string): Promise<ApiResult<ExampleListItem[]>> => {       
    return await apiClient.get(`/examples/${id}/similar`);     
};