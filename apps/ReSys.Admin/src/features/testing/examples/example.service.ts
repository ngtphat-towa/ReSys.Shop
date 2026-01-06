import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type {
    ExampleListItem,
    ExampleDetail,
    CreateExampleRequest,
    UpdateExampleRequest,
    ExampleQuery
} from './example.types';

/**
 * Fetches a paginated list of examples based on query filters.
 * @param query Optional filtering, sorting, and pagination parameters.
 * @returns A promise resolving to an ApiResult containing an array of ExampleListItem.
 */
export const getExamples = async (query?: ExampleQuery): Promise<ApiResult<ExampleListItem[]>> => {
    return await apiClient.get('/examples', { params: query });
};

/**
 * Retrieves the full details of a specific example by its ID.
 * @param id The unique identifier of the example.
 * @returns A promise resolving to an ApiResult containing ExampleDetail.
 */
export const getExampleById = async (id: string): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.get(`/testing/examples/${id}`);
};

/**
 * Creates a new example entry.
 * @param request The data for the new example.
 * @returns A promise resolving to an ApiResult containing the created ExampleDetail.
 */
export const createExample = async (request: CreateExampleRequest): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.post('/examples', request);
};

/**
 * Updates an existing example's basic information.
 * @param id The unique identifier of the example to update.
 * @param request The updated data.
 * @returns A promise resolving to an ApiResult containing the updated ExampleDetail.
 */
export const updateExample = async (id: string, request: UpdateExampleRequest): Promise<ApiResult<ExampleDetail>> => {
    return await apiClient.put(`/testing/examples/${id}`, request);
};

/**
 * Uploads or updates an image for a specific example.
 * @param id The unique identifier of the example.
 * @param file The image file to be uploaded.
 * @returns A promise resolving to an ApiResult containing the updated ExampleDetail.
 */
export const updateExampleImage = async (id: string, file: File): Promise<ApiResult<ExampleDetail>> => {
    const formData = new FormData();
    formData.append('image', file);
    return await apiClient.post(`/testing/examples/${id}/image`, formData, {
        headers: {
            'Content-Type': 'multipart/form-data',
        },
    });
};

/**
 * Deletes an example from the system.
 * @param id The unique identifier of the example to delete.
 * @returns A promise resolving to an ApiResult with no data.
 */
export const deleteExample = async (id: string): Promise<ApiResult<void>> => {
    return await apiClient.delete(`/testing/examples/${id}`);
};
