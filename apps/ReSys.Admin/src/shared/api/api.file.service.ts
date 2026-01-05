import apiClient from './api.client';
import type { ApiResult } from './api.types';
import type { FileMetadata, FileUploadResponse } from './api.file.types';

/**
 * Service for managing files and retrieving file metadata.
 */
export const fileService = {
    /**
     * Retrieves the metadata for a specific file by its path/id.
     * @param path The full path or file ID (e.g., 'products/image.webp').
     */
    async getFileMetadata(path: string): Promise<ApiResult<FileMetadata>> {
        return await apiClient.get(`/files/meta/${path}`);
    },

    /**
     * Uploads an image and returns processing metadata (dimensions, format, etc.).
     * @param file The image file to upload.
     */
    async uploadImage(file: File): Promise<ApiResult<FileUploadResponse>> {
        const formData = new FormData();
        formData.append('file', file);
        return await apiClient.post('/files/image', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
    }
};

export default fileService;
