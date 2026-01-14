import type { ApiResult } from '@/shared/api/api.types'
import type { ExampleCategoryFormData } from '../schemas/example-category.schema'

export interface ExampleCategoryListItem {
  id: string
  name: string
  description?: string
}

/** Detailed view of an example category. Currently identical to list item but separated for future extensibility. */
export type ExampleCategoryDetail = ExampleCategoryListItem

export type CreateExampleCategoryRequest = ExampleCategoryFormData
export type UpdateExampleCategoryRequest = ExampleCategoryFormData

export interface ExampleCategoryQuery {
  filter?: string
  sort?: string
  search?: string
  search_field?: string[]
  page?: number
  page_size?: number
}

export type { ApiResult }
