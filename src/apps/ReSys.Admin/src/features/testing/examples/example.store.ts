import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as exampleService from './example.service'
import type {
  ExampleListItem,
  ExampleDetail,
  CreateExampleRequest,
  UpdateExampleRequest,
  ExampleQuery,
} from './example.types'
import type { ApiResult } from '@/shared/api/api.types'

/**
 * Pinia store managing the state, actions, and data synchronization for Example features.
 * Handles loading states, pagination metadata, and CRUD orchestration.
 */
export const useExampleStore = defineStore('example', () => {
  // --- STATE ---
  const examples = ref<ExampleListItem[]>([])
  const currentExample = ref<ExampleDetail | null>(null)
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ExampleQuery>({
    page: 1,
    page_size: 10,
    sort_by: 'name',
    is_descending: false,
  })

  // --- ACTIONS ---

  /**
   * Fetches the list of examples from the API and updates the local state.
   * @param params Optional query parameters to override the current store query.
   */
  async function fetchExamples(params?: Partial<ExampleQuery>) {
    loading.value = true
    if (params) {
      query.value = { ...query.value, ...params }
    }

    const result = await exampleService.getExamples(query.value)
    if (result.success) {
      examples.value = result.data
      totalRecords.value = result.meta?.total_count || 0
    }
    loading.value = false
    return result
  }

  /**
   * Retrieves a single example by ID and sets it as the current active example.
   * @param id The unique identifier of the example.
   */
  async function fetchExampleById(id: string) {
    loading.value = true
    const result = await exampleService.getExampleById(id)
    if (result.success) {
      currentExample.value = result.data
    }
    loading.value = false
    return result
  }

  /**
   * Orchestrates the creation of a new example, including optional image upload.
   * @param request Basic information for the new example.
   * @param image Optional File object for the example's image.
   * @returns A promise resolving to an ApiResult containing the created ExampleDetail.
   */
  async function createExample(
    request: CreateExampleRequest,
    image?: File,
  ): Promise<ApiResult<ExampleDetail>> {
    loading.value = true
    const result = await exampleService.createExample(request)

    if (result.success && image) {
      const imageResult = await exampleService.updateExampleImage(result.data.id, image)
      if (imageResult.success) {
        result.data = imageResult.data
      }
    }

    loading.value = false
    return result
  }

  /**
   * Updates an existing example and its image if provided.
   * @param id The ID of the example to update.
   * @param request The updated data fields.
   * @param image Optional new image file.
   * @returns A promise resolving to an ApiResult containing the updated ExampleDetail.
   */
  async function updateExample(
    id: string,
    request: UpdateExampleRequest,
    image?: File,
  ): Promise<ApiResult<ExampleDetail>> {
    loading.value = true
    const result = await exampleService.updateExample(id, request)

    if (result.success && image) {
      const imageResult = await exampleService.updateExampleImage(id, image)
      if (imageResult.success) {
        result.data = imageResult.data
      }
    }

    loading.value = false
    return result
  }

  /**
   * Creates a copy of an existing example by fetching its details and re-creating it.
   * @param id The ID of the example to duplicate.
   * @returns A promise resolving to an ApiResult containing the new example.
   */
  async function duplicateExample(id: string) {
    loading.value = true
    const detail = await exampleService.getExampleById(id)
    if (!detail.success) {
      loading.value = false
      return detail
    }

    const { name, description, price, image_url, hex_color } = detail.data
    const result = await exampleService.createExample({
      name: `${name} (Copy)`,
      description,
      price,
      image_url: image_url || undefined,
      status: 0, // Reset to Draft for the copy
      hex_color,
    })

    if (result.success) {
      await fetchExamples()
    }
    loading.value = false
    return result
  }

  /**
   * Deletes an example and removes it from the local list if successful.
   * @param id The ID of the example to delete.
   * @returns A promise resolving to an ApiResult.
   */
  async function deleteExample(id: string) {
    loading.value = true
    const result = await exampleService.deleteExample(id)
    if (result.success) {
      examples.value = examples.value.filter((e) => e.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  /**
   * Resets the current example state.
   */
  function clearCurrent() {
    currentExample.value = null
  }

  return {
    examples,
    currentExample,
    loading,
    totalRecords,
    query,
    fetchExamples,
    fetchExampleById,
    createExample,
    updateExample,
    duplicateExample,
    deleteExample,
    clearCurrent,
  }
})
