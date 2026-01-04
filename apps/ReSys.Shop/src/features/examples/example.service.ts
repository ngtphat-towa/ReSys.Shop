import apiClient from '@/shared/api/api-client';
import type { 
    ApiResponse,
    ExampleListItem,
    ExampleDetail,
    ExampleQuery
} from './example.types';

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
