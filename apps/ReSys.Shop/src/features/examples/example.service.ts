import apiClient from '@/shared/api/client';
import type { ApiResult } from '@/shared/api/types';
import type { 
    ApiResponse,
    ExampleListItem,
    ExampleDetail,
    ExampleQuery
} from './example.types';

export const getExamples = async (query?: ExampleQuery): Promise<ApiResult<ExampleListItem[]>> => {    
    return await apiClient.get('/examples', { params: query });
};

export const getExampleById = async (id: string): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.get(`/examples/${id}`);
};

export const getSimilarExamples = async (id: string): Promise<ApiResult<ExampleListItem[]>> => {       
    return await apiClient.get(`/examples/${id}/similar`);     
};