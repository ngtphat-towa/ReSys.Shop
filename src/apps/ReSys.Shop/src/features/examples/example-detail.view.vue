<script setup lang="ts">
import { onMounted, watch, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useExampleStore } from './example.store';
import { storeToRefs } from 'pinia';
import { exampleLocales } from './example.locales';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue';

const route = useRoute();
const router = useRouter();
const exampleStore = useExampleStore();
const { currentExample, similarExamples, loading } = storeToRefs(exampleStore);
const { handleApiResult } = useApiErrorHandler();
const { formatCurrency } = useFormatter();

const dynamicStyle = computed(() => {
    if (currentExample.value?.hex_color) {
        return {
            '--p-primary-color': currentExample.value.hex_color,
            '--p-primary-500': currentExample.value.hex_color,
            '--p-button-primary-background': currentExample.value.hex_color,
            '--p-button-primary-border-color': currentExample.value.hex_color,
        };
    }
    return {};
});

const loadData = async (id: string) => {
    const result = await exampleStore.fetchExampleDetails(id);
    handleApiResult(result, {
        errorTitle: exampleLocales.common?.error,
        genericError: exampleLocales.messages?.load_error
    });
};

onMounted(() => {
    loadData(route.params.id as string);
});

watch(() => route.params.id, (newId) => {
    if (newId) {
        loadData(newId as string);
    }
});
</script>

<template>
    <div class="container p-4 mx-auto md:p-12" :style="dynamicStyle">
        <AppBreadcrumb :locales="exampleLocales" />

        <div v-if="loading && !currentExample" class="flex flex-col gap-16 animate-pulse">
            <div class="flex flex-col gap-16 lg:flex-row">
                <div class="w-full lg:w-1/2 aspect-square bg-surface-100 dark:bg-surface-800 rounded-[3rem]"></div>
                <div class="flex flex-col w-full gap-6 pt-8 lg:w-1/2">
                    <div class="w-3/4 h-16 bg-surface-100 dark:bg-surface-800 rounded-2xl"></div>
                    <div class="w-1/4 h-12 bg-surface-100 dark:bg-surface-800 rounded-2xl"></div>
                    <div class="w-full h-40 mt-8 bg-surface-100 dark:bg-surface-800 rounded-2xl"></div>
                </div>
            </div>
        </div>

        <div v-else-if="currentExample" class="flex flex-col gap-24">
            <!-- Example Section -->
            <div class="flex flex-col gap-20 lg:flex-row">
                <!-- Image Gallery -->
                <div class="w-full lg:w-1/2">
                    <div class="sticky top-28">
                        <div class="relative rounded-[3.5rem] overflow-hidden shadow-2xl bg-surface-0 dark:bg-surface-900 aspect-4/5 border border-surface-100 dark:border-surface-800 p-6">
                            <div class="w-full h-full rounded-[2.5rem] overflow-hidden bg-surface-50 dark:bg-surface-800">
                                <img v-if="currentExample.image_url" :src="currentExample.image_url" :alt="currentExample.name" class="object-cover w-full h-full" />
                                <div v-else class="flex items-center justify-center w-full h-full text-surface-200 dark:text-surface-700">
                                    <i class="pi pi-image text-9xl"></i>
                                </div>
                            </div>
                            <div class="absolute top-12 right-12">
                                <Tag severity="success" :value="exampleLocales.messages?.premium" class="px-6 py-2 text-sm font-black rounded-full shadow-2xl" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Example Info -->
                <div class="flex flex-col justify-center w-full lg:w-1/2">
                    <div class="mb-10">
                        <Button :label="exampleLocales.actions.back_to_catalog" icon="pi pi-arrow-left" text severity="secondary" @click="router.push({ name: 'shop.home' })" class="mb-6 font-bold" />

                        <div class="flex items-center gap-4 mb-4">
                            <h1 class="text-6xl font-black text-surface-900 dark:text-surface-50 leading-[1.1] tracking-tighter">{{ currentExample.name }}</h1>
                            <div v-if="currentExample.hex_color"
                                 class="w-10 h-10 border shadow-lg rounded-2xl border-surface-100 dark:border-surface-800 shrink-0"
                                 :style="{ backgroundColor: currentExample.hex_color }">
                            </div>
                        </div>
                        <div class="flex items-center gap-4">
                            <div class="flex text-yellow-400">
                                <i v-for="i in 5" :key="i" class="text-sm pi pi-star-fill"></i>
                            </div>
                            <span class="text-sm font-bold tracking-widest uppercase text-surface-400 dark:text-surface-500">{{ exampleLocales.labels?.rating_in_stock }}</span>
                        </div>
                    </div>

                    <div class="mb-12">
                        <span class="block mb-2 text-5xl font-black text-surface-900 dark:text-surface-50">
                            {{ formatCurrency(currentExample.price) }}
                        </span>
                        <p class="font-medium text-surface-500 dark:text-surface-400">{{ exampleLocales.labels?.free_delivery }}</p>
                    </div>

                    <Divider class="my-0" />

                    <div class="py-12">
                        <h3 class="mb-4 text-xl font-black tracking-widest uppercase text-surface-900 dark:text-surface-50">{{ exampleLocales.labels?.details }}</h3>
                        <p class="text-surface-600 dark:text-surface-300 leading-[1.8] text-lg whitespace-pre-line">{{ currentExample.description || exampleLocales.messages?.no_details_desc }}</p>
                    </div>

                    <div class="flex flex-col gap-4 mt-8 sm:flex-row">
                        <Button :label="exampleLocales.actions.add_to_bag" icon="pi pi-shopping-bag" class="flex-1 py-5 text-xl font-black shadow-2xl rounded-3xl shadow-primary/30" />
                        <Button icon="pi pi-heart" severity="secondary" outlined class="p-5 px-6 rounded-3xl border-surface-200 dark:border-surface-700" />
                    </div>

                    <div class="grid grid-cols-1 gap-6 mt-12 sm:grid-cols-2">
                        <div class="flex items-center gap-5 p-6 border bg-surface-50 dark:bg-surface-800 rounded-3xl border-surface-100 dark:border-surface-700">
                            <div class="flex items-center justify-center w-12 h-12 shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl text-primary">
                                <i class="text-xl pi pi-truck"></i>
                            </div>
                            <div>
                                <p class="font-black text-surface-900 dark:text-surface-0">{{ exampleLocales.labels?.shipping }}</p>
                                <p class="text-sm text-surface-500 dark:text-surface-400">{{ exampleLocales.messages?.shipping_desc }}</p>
                            </div>
                        </div>
                        <div class="flex items-center gap-5 p-6 border bg-surface-50 dark:bg-surface-800 rounded-3xl border-surface-100 dark:border-surface-700">
                            <div class="flex items-center justify-center w-12 h-12 shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl text-primary">
                                <i class="text-xl pi pi-shield"></i>
                            </div>
                            <div>
                                <p class="font-black text-surface-900 dark:text-surface-0">{{ exampleLocales.labels?.secure }}</p>
                                <p class="text-sm text-surface-500 dark:text-surface-400">{{ exampleLocales.messages?.secure_desc }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Similar Examples -->
            <div v-if="similarExamples.length > 0">
                <div class="flex items-end justify-between mb-12">
                    <div>
                        <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50">{{ exampleLocales.titles.recommendations_primary }} <span class="text-primary">{{ exampleLocales.titles.recommendations_highlight }}</span></h2>
                        <p class="mt-2 text-lg font-medium text-surface-500 dark:text-surface-400">{{ exampleLocales.titles.recommendations_desc }}</p>
                    </div>
                    <Button :label="exampleLocales.actions.explore_all" severity="secondary" text class="font-bold" />
                </div>
                <div class="grid grid-cols-2 gap-8 md:grid-cols-4 lg:grid-cols-5">
                    <Card v-for="sim in similarExamples" :key="sim.id" class="overflow-hidden border-none shadow-sm hover:shadow-xl transition-all cursor-pointer group rounded-[2.5rem] bg-surface-0 dark:bg-surface-900 border border-surface-50 dark:border-surface-800" @click="router.push({ name: 'shop.examples.detail', params: { id: sim.id } })">
                        <template #header>
                            <div class="aspect-square overflow-hidden bg-surface-50 dark:bg-surface-800 m-3 rounded-[1.8rem]">
                                <img v-if="sim.image_url" :src="sim.image_url" :alt="sim.name" class="object-cover w-full h-full transition-transform duration-700 group-hover:scale-110" />
                                <div v-else class="flex items-center justify-center w-full h-full text-surface-200 dark:text-surface-700">
                                    <i class="text-3xl pi pi-image"></i>
                                </div>
                            </div>
                        </template>
                        <template #title>
                            <span class="block px-2 text-base font-black transition-colors line-clamp-1 group-hover:text-primary text-surface-900 dark:text-surface-50">{{ sim.name }}</span>
                        </template>
                        <template #subtitle>
                            <span class="block px-2 pb-2 text-xl font-black text-surface-900 dark:text-surface-0">
                                {{ formatCurrency(sim.price) }}
                            </span>
                        </template>
                    </Card>
                </div>
            </div>
        </div>

        <div v-else class="text-center py-40 bg-surface-50 dark:bg-surface-800/50 rounded-[4rem] border-2 border-dashed border-surface-200 dark:border-surface-700">
            <div class="flex items-center justify-center w-24 h-24 mx-auto mb-8 rounded-full shadow-md bg-surface-0 dark:bg-surface-900">
                <i class="text-4xl pi pi-exclamation-triangle text-surface-300 dark:text-surface-600"></i>
            </div>
            <h2 class="mb-4 text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-0">{{ exampleLocales.messages?.not_found }}</h2>
            <p class="max-w-md mx-auto mb-12 text-lg text-surface-500 dark:text-surface-400">{{ exampleLocales.messages?.not_found_desc }}</p>
            <Button :label="exampleLocales.actions.return" icon="pi pi-home" @click="router.push({ name: 'shop.home' })" class="px-8 py-4 font-bold shadow-xl rounded-2xl shadow-primary/20" />
        </div>
    </div>
</template>

<style scoped>
:deep(.p-card-body) {
    padding: 0 1rem 1.5rem 1rem;
}
:deep(.p-card-title) {
    margin: 0;
}
</style>
