/**
 * Metadata for a file stored in the ReSys system.
 * Matches the backend FileMetadata record (snake_case).
 */
export interface FileMetadata {
    file_id: string;
    file_name: string;
    original_file_name: string;
    file_size: number;
    content_type: string;
    hash: string;
    subdirectory: string;
    created_at: string;
    extension?: string;
    is_encrypted: boolean;
    modified_at?: string;
    custom_metadata?: Record<string, string>;
}

/**
 * Response structure for image uploads.
 */
export interface FileUploadResponse {
    width: number;
    height: number;
    format: string;
    size_bytes: number;
    saved_name: string;
    url: string;
    hash: string;
}
