<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useExamplestore } from '@/stores/ExampleStore';
import { storeToRefs } from 'pinia';

const Examplestore = useExamplestore();
const { Examples, loading } = storeToRefs(Examplestore);
const search = ref('');

const loadExamples = async () => {
  await Examplestore.fetchExamples({ 
      search: search.value,
      page_size: 20 
  });
};

const onSearch = () => {
    loadExamples();
};

onMounted(() => {
    loadExamples();
});
</script>

<template>
  <div class="container mx-auto p-4 md:p-8">
    <!-- Hero Section / Search -->
    <div class="mb-12 flex flex-col md:flex-row justify-between items-center bg-surface-0 dark:bg-surface-900 p-8 md:p-12 rounded-[2.5rem] shadow-sm border border-surface-100 dark:border-surface-800 gap-8 transition-colors duration-300">
        <div class="text-center md:text-left">
            <h1 class="text-5xl font-black text-surface-900 dark:text-surface-0 tracking-tight leading-none mb-4">
                Exclusive <span class="text-primary">Collections</span>
            </h1>
            <p class="text-surface-500 dark:text-surface-400 text-lg max-w-md">Find the perfect items curated just for your lifestyle.</p>
        </div>
        <div class="flex gap-3 w-full max-w-lg">
            <IconField iconPosition="left" class="flex-1 shadow-sm rounded-2xl overflow-hidden">
                <InputIcon class="pi pi-search" />
                <InputText v-model="search" placeholder="Search our catalog..." class="w-full py-4 text-lg border-none bg-surface-50 dark:bg-surface-800 focus:bg-surface-0 dark:focus:bg-surface-950 transition-colors" @keyup.enter="onSearch" />
            </IconField>
            <Button icon="pi pi-search" class="p-4 px-6 rounded-2xl shadow-lg" @click="onSearch" />
        </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        <div v-for="i in 8" :key="i" class="bg-surface-0 dark:bg-surface-900 rounded-[2rem] overflow-hidden shadow-sm border border-surface-100 dark:border-surface-800 p-4">
            <Skeleton width="100%" height="250px" borderRadius="1.5rem" class="mb-4"></Skeleton>
            <Skeleton width="60%" height="1.5rem" class="mb-2"></Skeleton>
            <Skeleton width="40%" height="2rem" class="mb-4"></Skeleton>
            <Skeleton width="100%" height="3rem" borderRadius="1rem"></Skeleton>
        </div>
    </div>
    
    <!-- Empty State -->
    <div v-else-if="Examples.length === 0" class="text-center py-32 bg-surface-50 dark:bg-surface-800/50 rounded-[3rem] border-2 border-dashed border-surface-200 dark:border-surface-700">
        <div class="bg-surface-0 dark:bg-surface-900 w-24 h-24 rounded-full flex items-center justify-center mx-auto shadow-md mb-6">
            <i class="pi pi-search text-4xl text-surface-300 dark:text-surface-600"></i>
        </div>
        <h2 class="text-3xl font-black text-surface-800 dark:text-surface-100 mb-2">No results found</h2>
        <p class="text-surface-400 dark:text-surface-500 text-lg max-w-sm mx-auto">We couldn't find anything matching your search. Try another term!</p>
        <Button label="View All Examples" class="mt-8 p-button-text font-bold" @click="search = ''; onSearch()" />
    </div>

    <!-- Example Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-10">
        <Card v-for="Example in Examples" :key="Example.id" class="overflow-hidden border-none shadow-sm hover:shadow-2xl transition-all duration-500 transform hover:-translate-y-2 rounded-[2.5rem] group bg-surface-0 dark:bg-surface-900 border border-surface-50 dark:border-surface-800">
            <template #header>
                <div class="relative overflow-hidden aspect-[4/5] m-4 rounded-[2rem] shadow-inner bg-surface-50 dark:bg-surface-800">
                    <img v-if="Example.image_url" :src="Example.image_url" :alt="Example.name" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" />
                    <div v-else class="w-full h-full flex items-center justify-center text-surface-200 dark:text-surface-700 transition-colors">
                        <i class="pi pi-image text-6xl"></i>
                    </div>
                    <div class="absolute top-4 right-4">
                        <Button icon="pi pi-heart" severity="secondary" rounded text class="bg-surface-0/90 dark:bg-surface-900/90 backdrop-blur-md shadow-sm hover:text-red-500 w-12 h-12" />
                    </div>
                </div>
            </template>
            <template #title>
                <h3 class="text-xl font-black text-surface-900 dark:text-surface-0 line-clamp-1 mb-1 tracking-tight">{{ Example.name }}</h3>
            </template>
            <template #subtitle>
                <span class="text-3xl font-black text-primary">
                    {{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(Example.price) }}
                </span>
            </template>
            <template #content>
                <p class="text-surface-500 dark:text-surface-400 line-clamp-2 text-sm leading-relaxed mt-2 italic">{{ Example.description || 'No description available' }}</p>
            </template>
            <template #footer>
                <div class="flex gap-3 mt-4">
                    <Button label="Details" severity="secondary" outlined class="flex-1 py-3 rounded-2xl font-bold border-surface-200 dark:border-surface-700" @click="$router.push(`/Examples/${Example.id}`)" />
                    <Button label="Add" icon="pi pi-shopping-bag" class="flex-1 py-3 rounded-2xl font-bold shadow-lg shadow-primary/20" />
                </div>
            </template>
        </Card>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-card-body) {
    padding: 0 2rem 2rem 2rem;
}
:deep(.p-card-title) {
    margin: 0;
}
:deep(.p-card-subtitle) {
    margin: 0;
}
:deep(.p-card-footer) {
    padding-top: 0;
}
</style>