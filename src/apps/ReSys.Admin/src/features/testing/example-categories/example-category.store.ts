import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as categoryService from './example-category.service'
import type {
  ExampleCategoryListItem,
  ExampleCategoryDetail,
  CreateExampleCategoryRequest,
  UpdateExampleCategoryRequest,
  ExampleCategoryQuery,
} from './example-category.types'
import type { ApiResult } from '@/shared/api/api.types'

export const useExampleCategoryStore = defineStore('example-category', () => {
  const categories = ref<ExampleCategoryListItem[]>([])
  const currentCategory = ref<ExampleCategoryDetail | null>(null)
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ExampleCategoryQuery>({
    page: 1,
    page_size: 10,
    sort: 'name',
  })

  async function fetchCategories(params?: Partial<ExampleCategoryQuery>) {
    loading.value = true
    if (params) {
      query.value = { ...query.value, ...params }
    }

    const result = await categoryService.getExampleCategories(query.value)
    if (result.success) {
      categories.value = result.data
      totalRecords.value = result.meta?.total_count || 0
    }
    loading.value = false
    return result
  }

  async function fetchCategoryById(id: string) {
    loading.value = true
    const result = await categoryService.getExampleCategoryById(id)
    if (result.success) {
      currentCategory.value = result.data
    }
    loading.value = false
    return result
  }

  async function createCategory(
    request: CreateExampleCategoryRequest,
  ): Promise<ApiResult<ExampleCategoryDetail>> {
    loading.value = true
    const result = await categoryService.createExampleCategory(request)
    loading.value = false
    return result
  }

  async function updateCategory(
    id: string,
    request: UpdateExampleCategoryRequest,
  ): Promise<ApiResult<ExampleCategoryDetail>> {
    loading.value = true
    const result = await categoryService.updateExampleCategory(id, request)
    loading.value = false
    return result
  }

  async function deleteCategory(id: string) {
    loading.value = true
    const result = await categoryService.deleteExampleCategory(id)
    if (result.success) {
      categories.value = categories.value.filter((c) => c.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  function clearCurrent() {
    currentCategory.value = null
  }

  return {
    categories,
    currentCategory,
    loading,
    totalRecords,
    query,
    fetchCategories,
    fetchCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
    clearCurrent,
  }
})
