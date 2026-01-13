import type { ApiResult } from '@/shared/api/api.types'
import type { ExampleFormData } from './example.schema'

/**
 * Shared types for the Example feature
 */
export enum ExampleStatus {
  Draft = 0,
  Active = 1,
  Archived = 2,
}

/**
 * Maps ExampleStatus to semantic Tailwind color classes or PrimeVue severities.
 * Uses semantic variable classes from tailwindcss-primeui to avoid hardcoding.
 */
export const STATUS_COLORS: Record<
  ExampleStatus,
  {
    text: string
    bg: string
    fill: string
    contrast: string
    dot: string
    severity: 'success' | 'warning' | 'secondary' | 'info' | 'danger' | 'warn' | undefined
  }
> = {
  [ExampleStatus.Draft]: {
    text: 'text-warn',
    bg: 'bg-warn-contrast',
    fill: 'bg-warn',
    contrast: 'text-warn-contrast',
    dot: 'bg-warn',
    severity: 'warn',
  },
  [ExampleStatus.Active]: {
    text: 'text-success',
    bg: 'bg-success-contrast',
    fill: 'bg-success',
    contrast: 'text-success-contrast',
    dot: 'bg-success',
    severity: 'success',
  },
  [ExampleStatus.Archived]: {
    text: 'text-surface-500',
    bg: 'bg-surface-100',
    fill: 'bg-surface-500',
    contrast: 'text-surface-0',
    dot: 'bg-surface-500',
    severity: 'secondary',
  },
}

export interface ExampleListItem {
  id: string
  name: string
  description?: string
  price: number
  image_url?: string
  status: ExampleStatus
  hex_color?: string
  category_id?: string
  category_name?: string
}

export interface ExampleDetail extends ExampleListItem {
  created_at: string | Date
}

/**
 * Semantic requests
 */
export type CreateExampleRequest = ExampleFormData
export type UpdateExampleRequest = ExampleFormData

export interface ExampleQuery {
  search?: string
  search_field?: string[]
  filter?: string
  sort?: string
  page?: number
  page_size?: number
  // Legacy fields (optional if middleware handles them)
  name?: string
  min_price?: number
  max_price?: number
  status?: ExampleStatus[]
  created_from?: string
  created_to?: string
  sort_by?: string
  is_descending?: boolean
}

export type { ApiResult }
