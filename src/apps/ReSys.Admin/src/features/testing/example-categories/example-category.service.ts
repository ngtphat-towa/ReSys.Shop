import apiClient from '@/shared/api/api.client'
import type { ApiResult } from '@/shared/api/api.types'
import type {
  ExampleCategoryListItem,
  ExampleCategoryDetail,
  CreateExampleCategoryRequest,
  UpdateExampleCategoryRequest,
  ExampleCategoryQuery,
} from './example-category.types'

export const getExampleCategories = async (
  query?: ExampleCategoryQuery,
): Promise<ApiResult<ExampleCategoryListItem[]>> => {
  return await apiClient.get('/testing/example-categories', { params: query })
}

export const getExampleCategoryById = async (
  id: string,
): Promise<ApiResult<ExampleCategoryDetail>> => {
  return await apiClient.get(`/testing/example-categories/${id}`)
}

export const createExampleCategory = async (
  request: CreateExampleCategoryRequest,
): Promise<ApiResult<ExampleCategoryDetail>> => {
  return await apiClient.post('/testing/example-categories', request)
}

export const updateExampleCategory = async (
  id: string,
  request: UpdateExampleCategoryRequest,
): Promise<ApiResult<ExampleCategoryDetail>> => {
  return await apiClient.put(`/testing/example-categories/${id}`, request)
}

export const deleteExampleCategory = async (id: string): Promise<ApiResult<void>> => {
  return await apiClient.delete(`/testing/example-categories/${id}`)
}
