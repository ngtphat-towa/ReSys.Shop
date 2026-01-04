import { ref, onUnmounted } from 'vue';

/**
 * Handles file selection, local preview URL generation, and cleanup.
 */
export function useFilePreview() {
    const file = ref<File | null>(null);
    const previewUrl = ref<string | null>(null);

    const setFile = (newFile: File | null) => {
        // Cleanup previous preview
        if (previewUrl.value) {
            URL.revokeObjectURL(previewUrl.value);
            previewUrl.value = null;
        }

        file.value = newFile;

        if (newFile) {
            previewUrl.value = URL.createObjectURL(newFile);
        }
    };

    const clear = () => setFile(null);

    // Ensure memory is released when component is destroyed
    onUnmounted(() => {
        if (previewUrl.value) {
            URL.revokeObjectURL(previewUrl.value);
        }
    });

    return {
        file,
        previewUrl,
        setFile,
        clear
    };
}
