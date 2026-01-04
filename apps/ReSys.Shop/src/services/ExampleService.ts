import apiClient from './apiClient';
import type { ApiResponse } from '@/types/api';
import type { 
    ExampleListItem, 
    ExampleDetail, 
    ExampleQuery 
} from '@/types/example';

export const getExamples = async (query?: ExampleQuery): Promise<ApiResponse<ExampleListItem[]>> => {    
    const response = await apiClient.get<ApiResponse<ExampleListItem[]>>('/examples', { params: query });
    return response.data;
};

export const getExampleById = async (id: string): Promise<ApiResponse<ExampleDetail>> => {
    const response = await apiClient.get<ApiResponse<ExampleDetail>>(`/examples/${id}`);
    return response.data;
};

export const getSimilarExamples = async (id: string): Promise<ApiResponse<ExampleListItem[]>> => {       
    const response = await apiClient.get<ApiResponse<ExampleListItem[]>>(`/examples/${id}/similar`);     
    return response.data;
};