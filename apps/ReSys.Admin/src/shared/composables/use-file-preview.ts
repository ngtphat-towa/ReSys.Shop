import { ref, onUnmounted } from 'vue'

/**
 * Composable to manage local image file previews.
 * Handles the creation and memory cleanup of Object URLs.
 *
 * @example
 * const { previewUrl, handleFileChange, clearPreview } = useFilePreview();
 * <img :src="previewUrl" />
 */
export function useFilePreview() {
  /** The local Object URL generated for the preview. */
  const previewUrl = ref<string | null>(null)
  /** The actual File object selected by the user. */
  const selectedFile = ref<File | null>(null)

  /**
   * Updates the preview with a new file.
   * Revokes previous Object URLs to prevent memory leaks.
   * @param file The file to preview.
   */
  const handleFileChange = (file: File | null) => {
    if (previewUrl.value) {
      URL.revokeObjectURL(previewUrl.value)
    }

    selectedFile.value = file

    if (file) {
      previewUrl.value = URL.createObjectURL(file)
    } else {
      previewUrl.value = null
    }
  }

  /**
   * Clears both the file and the generated preview URL.
   */
  const clearPreview = () => {
    handleFileChange(null)
  }

  // Auto-cleanup when the component using this composable is destroyed
  onUnmounted(() => {
    if (previewUrl.value) {
      URL.revokeObjectURL(previewUrl.value)
    }
  })

  return {
    previewUrl,
    selectedFile,
    handleFileChange,
    clearPreview,
  }
}
