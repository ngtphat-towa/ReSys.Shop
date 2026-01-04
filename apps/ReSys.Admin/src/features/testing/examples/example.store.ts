import { defineStore } from 'pinia';
import { ref } from 'vue';
import {
    getExamples,
    getExampleById,
    createExample as apiCreateExample,
    updateExample as apiUpdateExample,
    deleteExample as apiDeleteExample,
    updateExampleImage as apiUpdateExampleImage
} from './example.service';
import type {
    ExampleListItem,
    ExampleDetail,
    ExampleQuery,
    CreateExampleRequest,
    UpdateExampleRequest
} from './example.types';

export const useExampleStore = defineStore('example', () => {
    const examples = ref<ExampleListItem[]>([]);
    const currentExample = ref<ExampleDetail | null>(null);
    const loading = ref(false);
    const totalRecords = ref(0);

    // Unified query state
    const query = ref<ExampleQuery>({
        page: 1,
        page_size: 10,
        search: '',
        sort_by: 'name',
        is_descending: false
    });

    async function fetchExamples(newQuery?: Partial<ExampleQuery>) {
        loading.value = true;

        if (newQuery) {
            query.value = { ...query.value, ...newQuery };
        }

        try {
            const params = Object.fromEntries(
                Object.entries(query.value).filter(([_, v]) => v != null && v !== '')
            );

            const result = await getExamples(params as ExampleQuery);
            if (result.success) {
                examples.value = result.data?.data;
                totalRecords.value = result.data.meta?.total_count ?? 0;
            }
            return result;
        } finally {
            loading.value = false;
        }
    }

    async function fetchExampleById(id: string) {
        loading.value = true;
        try {
            const result = await getExampleById(id);
            if (result.success) {
                currentExample.value = result.data.data;
            }
            return result;
        } finally {
            loading.value = false;
        }
    }

    async function createExample(request: CreateExampleRequest, image?: File) {
        loading.value = true;
        try {
            let result = await apiCreateExample(request);
            if (result.success && image && result.data.data.id) {
                result = await apiUpdateExampleImage(result.data.data.id, image);
            }
            return result;
        } finally {
            loading.value = false;
        }
    }

    async function updateExample(id: string, request: UpdateExampleRequest, image?: File) {
        loading.value = true;
        try {
            let result = await apiUpdateExample(id, request);
            if (result.success && image) {
                result = await apiUpdateExampleImage(id, image);
            }
            return result;
        } finally {
            loading.value = false;
        }
    }

    async function deleteExample(id: string) {
        loading.value = true;
        try {
            const result = await apiDeleteExample(id);
            if (result.success) {
                await fetchExamples();
            }
            return result;
        } finally {
            loading.value = false;
        }
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
        deleteExample
    };
});
