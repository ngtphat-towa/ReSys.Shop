<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue';

interface Props {
    modelValue: string | null;
    locales: any;
}

const props = defineProps<Props>();
const emit = defineEmits(['update:modelValue', 'fileChange']);

const fileInputRef = ref<HTMLInputElement | null>(null);
const imageFile = ref<File | null>(null);
const localPreviewUrl = ref<string | null>(null);

const previewSource = computed(() => localPreviewUrl.value || props.modelValue || null);

const currentFilename = computed(() => {
    if (imageFile.value) return imageFile.value.name;
    if (props.modelValue) {
        return props.modelValue.split('/').pop() || 'Existing Image';
    }
    return null;
});

const triggerFileUpload = () => fileInputRef.value?.click();

const onFileChange = (event: Event) => {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
        const file = input.files[0];
        if (localPreviewUrl.value) URL.revokeObjectURL(localPreviewUrl.value);
        imageFile.value = file;
        localPreviewUrl.value = URL.createObjectURL(file);
        emit('fileChange', file);
    }
};

const clearFileSelection = (event?: Event) => {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }

    imageFile.value = null;
    if (localPreviewUrl.value) {
        URL.revokeObjectURL(localPreviewUrl.value);
        localPreviewUrl.value = null;
    }
    if (fileInputRef.value) fileInputRef.value.value = '';

    emit('update:modelValue', null);
    emit('fileChange', null);
};

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

                <div class="flex flex-col gap-2">
                    <input type="file" ref="fileInputRef" @change="onFileChange" class="hidden" accept="image/*" />
                    <Button type="button" icon="pi pi-camera" severity="secondary" outlined :label="previewSource ? 'Change Image' : locales.placeholders?.upload" @click="triggerFileUpload" class="w-full rounded-xl" />
                    <div v-if="currentFilename" class="text-[10px] text-surface-500 truncate px-2 text-center font-bold italic">{{ currentFilename }}</div>
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
