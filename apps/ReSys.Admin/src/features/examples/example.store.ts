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
    const pagination = ref({
        page: 1,
        pageSize: 10
    });

    async function fetchExamples(query?: ExampleQuery) {
        loading.value = true;
        try {
            const response = await getExamples({
                page: pagination.value.page,
                page_size: pagination.value.pageSize,
                ...query
            });
            examples.value = response.data;
            totalRecords.value = response.meta?.total_count ?? 0;
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function fetchExampleById(id: string) {
        loading.value = true;
        try {
            const response = await getExampleById(id);
            currentExample.value = response.data;
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function createExample(request: CreateExampleRequest, image?: File) {
        loading.value = true;
        try {
            let response = await apiCreateExample(request);
            if (image && response.data.id) {
                response = await apiUpdateExampleImage(response.data.id, image);
            }
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function updateExample(id: string, request: UpdateExampleRequest, image?: File) {
        loading.value = true;
        try {
            let response = await apiUpdateExample(id, request);
            if (image) {
                response = await apiUpdateExampleImage(id, image);
            }
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function deleteExample(id: string) {
        loading.value = true;
        try {
            await apiDeleteExample(id);
            await fetchExamples();
        } finally {
            loading.value = false;
        }
    }

    return {
        examples,
        currentExample,
        loading,
        totalRecords,
        pagination,
        fetchExamples,
        fetchExampleById,
        createExample,
        updateExample,
        deleteExample
    };
});