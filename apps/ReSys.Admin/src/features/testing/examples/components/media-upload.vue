<script setup lang="ts">
import { ref, computed, onUnmounted, watch } from 'vue';
import { fileService } from '@/shared/api/file.service';
import type { FileMetadata } from '@/shared/api/file.types';

/**
 * Component Props
 * @property modelValue The current image URL (from server state).
 * @property locales Object containing localized strings for labels and messages.
 */
interface Props {
    modelValue: string | null;
    locales: any;
}

const props = defineProps<Props>();

/**
 * Component Emits
 * @emits update:modelValue Triggers when the image URL is cleared.
 * @emits fileChange Triggers when a new local file is selected or cleared.
 */
const emit = defineEmits(['update:modelValue', 'fileChange']);

// --- INTERNAL STATE ---
const fileInputRef = ref<HTMLInputElement | null>(null);
const imageFile = ref<File | null>(null);
const localPreviewUrl = ref<string | null>(null);
const metadata = ref<FileMetadata | null>(null);
const loadingMeta = ref(false);

/**
 * Determines the image source for the preview.
 * Priority: Locally selected file (Object URL) > Server-provided URL.
 */
const previewSource = computed(() => localPreviewUrl.value || props.modelValue || null);

/**
 * Fetches metadata if a server URL is provided.
 */
const fetchMetadata = async (url: string) => {
    // Robust extraction: find the index of /api/files/ and take everything after it
    const marker = '/api/files/';
    const index = url.indexOf(marker);
    if (index === -1) return;

    const path = url.substring(index + marker.length);
    if (!path) return;

    loadingMeta.value = true;
    const result = await fileService.getFileMetadata(path);
    if (result.success) {
        metadata.value = result.data;
    }
    loadingMeta.value = false;
};

// Watch for external modelValue changes to refresh metadata
watch(() => props.modelValue, (newVal) => {
    if (newVal && !localPreviewUrl.value) {
        fetchMetadata(newVal);
    } else if (!newVal) {
        metadata.value = null;
    }
}, { immediate: true });

/**
 * Computes a displayable filename.
 */
const currentFilename = computed(() => {
    if (imageFile.value) return imageFile.value.name;
    if (metadata.value) return metadata.value.original_file_name;
    if (props.modelValue) {
        return props.modelValue.split('/').pop() || 'Existing Image';
    }
    return null;
});

/**
 * Computes the display format (e.g., WEBP, PNG).
 * Prioritizes extension field, then derives from content_type.
 */
const displayFormat = computed(() => {
    if (metadata.value?.extension) return metadata.value.extension.replace('.', '');
    if (metadata.value?.content_type) return metadata.value.content_type.split('/').pop();
    if (imageFile.value) return imageFile.value.type.split('/').pop();
    return null;
});

/**
 * Formats a date string strictly as ISO 8601 UTC.
 * Ensures the 'Z' suffix is present for consistent parsing.
 */
const formatDisplayDate = (dateStr?: string) => {
    if (!dateStr || dateStr.startsWith('0001')) return null;
    
    // Ensure UTC format if missing 'Z' or offset
    const utcDateStr = (dateStr.endsWith('Z') || dateStr.includes('+')) 
        ? dateStr 
        : `${dateStr}Z`;
        
    const date = new Date(utcDateStr);
    if (isNaN(date.getTime())) return null;

    return date.toLocaleString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

/**
 * Formats bytes to readable size
 */
const formatSize = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

/**
 * Programmatically triggers the hidden file input.
 */
const triggerFileUpload = () => fileInputRef.value?.click();

/**
 * Handles the native file input change event.
 */
const onFileChange = (event: Event) => {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
        const file = input.files[0];
        if (localPreviewUrl.value) URL.revokeObjectURL(localPreviewUrl.value);
        
        imageFile.value = file;
        localPreviewUrl.value = URL.createObjectURL(file);
        metadata.value = null; // Clear server metadata for local file
        emit('fileChange', file);
    }
};

/**
 * Resets the component state.
 */
const clearFileSelection = (event?: Event) => {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }

    imageFile.value = null;
    metadata.value = null;
    if (localPreviewUrl.value) {
        URL.revokeObjectURL(localPreviewUrl.value);
        localPreviewUrl.value = null;
    }
    if (fileInputRef.value) fileInputRef.value.value = '';

    emit('update:modelValue', null);
    emit('fileChange', null);
};

// Lifecycle cleanup: ensure any created Object URLs are revoked
onUnmounted(() => {
    if (localPreviewUrl.value) URL.revokeObjectURL(localPreviewUrl.value);
});
</script>

<template>
    <Card class="overflow-hidden border border-none shadow-sm rounded-2xl border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900">
        <template #title>
            <div class="flex items-center gap-2">
                <i class="pi pi-image text-primary"></i>
                <span class="text-xl font-bold text-surface-800 dark:text-surface-50">{{ locales.labels?.media }}</span>
                <i class="text-sm pi pi-info-circle text-surface-400 cursor-help" v-tooltip="locales.tooltips?.media"></i>
            </div>
        </template>
        <template #content>
            <div class="flex flex-col gap-6 pt-2">
                <div class="relative p-1 overflow-hidden border-2 border-dashed group rounded-xl aspect-square border-surface-200 dark:border-surface-700 bg-surface-50 dark:bg-surface-800">
                    <template v-if="previewSource">
                        <Image
                            :src="previewSource"
                            preview
                            alt="Product Preview"
                            class="w-full h-full"
                            imageClass="w-full h-full object-cover rounded-lg"
                        />
                        <Button
                            type="button"
                            icon="pi pi-times"
                            severity="danger"
                            rounded
                            class="absolute! top-2 right-2 z-10 w-8 h-8 shadow-lg"
                            @click="clearFileSelection"
                        />
                    </template>
                    <div v-else class="flex flex-col items-center justify-center h-full text-surface-400">
                        <i class="mb-3 text-5xl pi pi-image opacity-20"></i>
                        <span class="text-sm font-medium">{{ locales.messages?.no_image }}</span>
                    </div>

                    <div v-if="imageFile" class="absolute bottom-2 left-2 right-2 bg-primary text-white text-[10px] uppercase font-bold py-1 px-3 rounded-lg text-center shadow-lg">
                        {{ locales.messages?.ready_to_upload }}
                    </div>
                </div>

                <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-2">
                        <input type="file" ref="fileInputRef" @change="onFileChange" class="hidden" accept="image/*" />
                        <Button type="button" icon="pi pi-camera" severity="secondary" outlined :label="previewSource ? 'Change Image' : locales.placeholders?.upload" @click="triggerFileUpload" class="w-full rounded-xl" />
                        <div v-if="currentFilename" class="text-[10px] text-surface-500 truncate px-2 text-center font-bold italic">{{ currentFilename }}</div>
                    </div>

                    <!-- Metadata Details Section -->
                    <div v-if="metadata || imageFile" class="p-4 rounded-xl bg-surface-50 dark:bg-surface-800 border border-surface-100 dark:border-surface-700">
                        <div class="flex flex-col gap-2 text-xs">
                            <div class="flex justify-between items-center">
                                <span class="text-surface-500 font-medium">{{ locales.labels?.file_size }}</span>
                                <span class="text-surface-900 dark:text-surface-0 font-bold">{{ imageFile ? formatSize(imageFile.size) : formatSize(metadata?.file_size || 0) }}</span>
                            </div>
                            <div v-if="displayFormat" class="flex justify-between items-center">
                                <span class="text-surface-500 font-medium">{{ locales.labels?.file_type }}</span>
                                <span class="text-surface-900 dark:text-surface-0 font-bold uppercase">{{ displayFormat }}</span>
                            </div>
                            <div v-if="formatDisplayDate(metadata?.created_at)" class="flex justify-between items-center">
                                <span class="text-surface-500 font-medium">{{ locales.labels?.uploaded_at }}</span>
                                <span class="text-surface-900 dark:text-surface-0 font-bold">{{ formatDisplayDate(metadata!.created_at) }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </Card>
</template>

<style scoped>
:deep(.p-image), :deep(.p-image > img) {
    width: 100%;
    height: 100%;
    display: block;
}
</style>