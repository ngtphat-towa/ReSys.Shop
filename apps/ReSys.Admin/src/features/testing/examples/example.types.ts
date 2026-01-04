import type { ApiResponse } from '@/shared/api/types'
import type { ExampleFormData } from './example.validator'

/**
 * Shared types for the Example feature
 */
export interface ExampleListItem extends ExampleFormData {
  id: string
}

export interface ExampleDetail extends ExampleFormData {
  id: string
  created_at: string
}

/**
 * Semantic requests
 */
export interface CreateExampleRequest extends ExampleFormData {}
export interface UpdateExampleRequest extends ExampleFormData {}

export interface ExampleQuery {
  search?: string
  name?: string
  min_price?: number
  max_price?: number
  created_from?: string
  created_to?: string
  sort_by?: string
  is_descending?: boolean
  page?: number
  page_size?: number
}

export type { ApiResponse }