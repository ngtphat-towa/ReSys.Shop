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
        
        // Update local query state if new params provided
        if (newQuery) {
            query.value = { ...query.value, ...newQuery };
        }

        try {
            const response = await getExamples(query.value);
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
        query,
        fetchExamples,
        fetchExampleById,
        createExample,
        updateExample,
        deleteExample
    };
});
