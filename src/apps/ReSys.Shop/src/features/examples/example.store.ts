import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getExamples, getExampleById, getSimilarExamples } from './example.service'
import type { ExampleListItem, ExampleDetail, ExampleQuery } from './example.types'

/**
 * Store for managing example items within the Shop application.
 * Focused on catalog browsing, product details, and recommendations.
 */
export const useExampleStore = defineStore('example', () => {
  // --- STATE ---
  const examples = ref<ExampleListItem[]>([])
  const currentExample = ref<ExampleDetail | null>(null)
  const similarExamples = ref<ExampleListItem[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)

  // --- ACTIONS ---

  /**
   * Fetches items for the shop catalog view.
   * @param query Optional filtering and pagination parameters.
   */
  async function fetchExamples(query?: ExampleQuery) {
    loading.value = true
    try {
      const result = await getExamples(query)
      if (result.success) {
        // Optimized: Accessing .data directly after interceptor unwrapping
        examples.value = result.data
        totalRecords.value = result.meta?.total_count ?? 0
      }
      return result
    } finally {
      loading.value = false
    }
  }

  /**
   * Fetches details for a product page and its similar recommendations.
   * @param id The unique identifier of the item.
   */
  async function fetchExampleDetails(id: string) {
    loading.value = true
    try {
      const [exampleRes, similarRes] = await Promise.all([
        getExampleById(id),
        getSimilarExamples(id),
      ])

      if (exampleRes.success) {
        currentExample.value = exampleRes.data
      }
      if (similarRes.success) {
        similarExamples.value = similarRes.data
      }

      return exampleRes
    } finally {
      loading.value = false
    }
  }

  return {
    examples,
    currentExample,
    similarExamples,
    loading,
    totalRecords,
    fetchExamples,
    fetchExampleDetails,
  }
})
