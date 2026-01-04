import { defineStore } from 'pinia';
import { ref } from 'vue';
import { getExamples, getExampleById, getSimilarExamples } from './example.service';
import type { ExampleListItem, ExampleDetail, ExampleQuery } from './example.types';

export const useExampleStore = defineStore('Example', () => {
    const Examples = ref<ExampleListItem[]>([]);
    const currentExample = ref<ExampleDetail | null>(null);
    const similarExamples = ref<ExampleListItem[]>([]);
    const loading = ref(false);
    const totalRecords = ref(0);

    async function fetchExamples(query?: ExampleQuery) {
        loading.value = true;
        try {
            const result = await getExamples(query);
            if (result.success) {
                Examples.value = result.data.data;
                totalRecords.value = result.data.meta?.total_count ?? 0;
            }
            return result;
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
            
            if (ExampleRes.success) {
                currentExample.value = ExampleRes.data.data;
            }
            if (similarRes.success) {
                similarExamples.value = similarRes.data.data;
            }
            
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
