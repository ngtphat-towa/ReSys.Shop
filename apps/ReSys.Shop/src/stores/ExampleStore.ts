import { defineStore } from 'pinia';
import { ref } from 'vue';
import { getExamples, getExampleById, getSimilarExamples } from '@/services/ExampleService';
import type { ExampleListItem, ExampleDetail, ExampleQuery } from '@/types/Example';

export const useExamplestore = defineStore('Example', () => {
    const Examples = ref<ExampleListItem[]>([]);
    const currentExample = ref<ExampleDetail | null>(null);
    const similarExamples = ref<ExampleListItem[]>([]);
    const loading = ref(false);
    const totalRecords = ref(0);

    async function fetchExamples(query?: ExampleQuery) {
        loading.value = true;
        try {
            const response = await getExamples(query);
            Examples.value = response.data;
            totalRecords.value = response.meta?.total_count ?? 0;
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function fetchExampleDetails(id: string) {
        loading.value = true;
        try {
            const [ExampleRes, similarRes] = await Promise.all([
                getExampleById(id),
                getSimilarExamples(id)
            ]);
            currentExample.value = ExampleRes.data;
            similarExamples.value = similarRes.data;
            return ExampleRes;
        } finally {
            loading.value = false;
        }
    }

    return {
        Examples,
        currentExample,
        similarExamples,
        loading,
        totalRecords,
        fetchExamples,
        fetchExampleDetails
    };
});
