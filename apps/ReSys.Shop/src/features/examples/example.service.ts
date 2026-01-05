import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type { 
    ExampleListItem,
    ExampleDetail,
    ExampleQuery
} from './example.types';

/**
 * Fetches a list of examples for the shop catalog.
 * @param query Filtering and pagination parameters.
 * @returns A promise resolving to an ApiResult with an array of ExampleListItem.
 */
export const getExamples = async (query?: ExampleQuery): Promise<ApiResult<ExampleListItem[]>> => {    
    return await apiClient.get('/examples', { params: query });
};

/**
 * Retrieves specific example details for the product page.
 * @param id The unique identifier of the example.
 * @returns A promise resolving to an ApiResult containing ExampleDetail.
 */
export const getExampleById = async (id: string): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.get(`/examples/${id}`);
};

/**
 * Retrieves similar examples for recommendations.
 * @param id The base example ID.
 * @returns A promise resolving to an ApiResult with similar items.
 */
export const getSimilarExamples = async (id: string): Promise<ApiResult<ExampleListItem[]>> => {       
    return await apiClient.get(`/examples/${id}/similar`);     
};