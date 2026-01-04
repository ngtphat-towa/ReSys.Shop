<script setup lang="ts">
import { onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useExamplestore } from '@/stores/ExampleStore';
import { storeToRefs } from 'pinia';
import Button from 'primevue/button';
import Card from 'primevue/card';
import Divider from 'primevue/divider';
import Tag from 'primevue/tag';
import Skeleton from 'primevue/skeleton';

const route = useRoute();
const router = useRouter();
const Examplestore = useExamplestore();
const { currentExample, similarExamples, loading } = storeToRefs(Examplestore);

const loadData = async (id: string) => {
    try {
        await Examplestore.fetchExampleDetails(id);
    } catch (e) {
        // Handled
    }
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
    <div class="container mx-auto p-4 md:p-12">
        <div v-if="loading && !currentExample" class="flex flex-col gap-16 animate-pulse">
            <div class="flex flex-col lg:flex-row gap-16">
                <div class="w-full lg:w-1/2 aspect-square bg-gray-100 rounded-[3rem]"></div>
                <div class="w-full lg:w-1/2 flex flex-col gap-6 pt-8">
                    <div class="h-16 bg-gray-100 rounded-2xl w-3/4"></div>
                    <div class="h-12 bg-gray-100 rounded-2xl w-1/4"></div>
                    <div class="h-40 bg-gray-100 rounded-2xl w-full mt-8"></div>
                </div>
            </div>
        </div>

        <div v-else-if="currentExample" class="flex flex-col gap-24">
            <!-- Example Section -->
            <div class="flex flex-col lg:flex-row gap-20">
                <!-- Image Gallery -->
                <div class="w-full lg:w-1/2">
                    <div class="sticky top-28">
                        <div class="relative rounded-[3.5rem] overflow-hidden shadow-2xl bg-white aspect-[4/5] border border-gray-100 p-6">
                            <div class="w-full h-full rounded-[2.5rem] overflow-hidden bg-gray-50">
                                <img v-if="currentExample.image_url" :src="currentExample.image_url" :alt="currentExample.name" class="w-full h-full object-cover" />
                                <div v-else class="w-full h-full flex items-center justify-center text-gray-200">
                                    <i class="pi pi-image text-9xl"></i>
                                </div>
                            </div>
                            <div class="absolute top-12 right-12">
                                <Tag severity="success" value="Premium Quality" class="px-6 py-2 text-sm font-black shadow-2xl rounded-full" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Example Info -->
                <div class="w-full lg:w-1/2 flex flex-col justify-center">
                    <div class="mb-10">
                        <Button label="Back to Catalog" icon="pi pi-arrow-left" text severity="secondary" @click="router.push('/')" class="mb-6 font-bold" />
                        <h1 class="text-6xl font-black text-gray-900 leading-[1.1] mb-4 tracking-tighter">{{ currentExample.name }}</h1>
                        <div class="flex items-center gap-4">
                            <div class="flex text-yellow-400">
                                <i v-for="i in 5" :key="i" class="pi pi-star-fill text-sm"></i>
                            </div>
                            <span class="text-gray-400 text-sm font-bold uppercase tracking-widest">In Stock & Ready to ship</span>
                        </div>
                    </div>

                    <div class="mb-12">
                        <span class="text-5xl font-black text-primary block mb-2">
                            {{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(currentExample.price) }}
                        </span>
                        <p class="text-gray-500 font-medium">Free express delivery on this item.</p>
                    </div>

                    <Divider class="my-0" />

                    <div class="py-12">
                        <h3 class="text-xl font-black text-gray-900 mb-4 uppercase tracking-widest">The Details</h3>
                        <p class="text-gray-600 leading-[1.8] text-lg whitespace-pre-line">{{ currentExample.description }}</p>
                    </div>

                    <div class="flex flex-col sm:flex-row gap-4 mt-8">
                        <Button label="Add to Shopping Bag" icon="pi pi-shopping-bag" class="flex-1 py-5 text-xl font-black rounded-3xl shadow-2xl shadow-primary/30" />
                        <Button icon="pi pi-heart" severity="secondary" outlined class="p-5 px-6 rounded-3xl border-gray-200" />
                    </div>

                    <div class="mt-12 grid grid-cols-1 sm:grid-cols-2 gap-6">
                        <div class="p-6 bg-gray-50 rounded-3xl border border-gray-100 flex items-center gap-5">
                            <div class="w-12 h-12 bg-white rounded-2xl flex items-center justify-center shadow-sm text-primary">
                                <i class="pi pi-truck text-xl"></i>
                            </div>
                            <div>
                                <p class="font-black text-gray-900">Fast Shipping</p>
                                <p class="text-sm text-gray-500">Arrives in 2-3 days</p>
                            </div>
                        </div>
                        <div class="p-6 bg-gray-50 rounded-3xl border border-gray-100 flex items-center gap-5">
                            <div class="w-12 h-12 bg-white rounded-2xl flex items-center justify-center shadow-sm text-primary">
                                <i class="pi pi-shield text-xl"></i>
                            </div>
                            <div>
                                <p class="font-black text-gray-900">Secure Store</p>
                                <p class="text-sm text-gray-500">100% money back</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Similar Examples -->
            <div v-if="similarExamples.length > 0">
                <div class="flex justify-between items-end mb-12">
                    <div>
                        <h2 class="text-4xl font-black text-gray-900 tracking-tighter">You May Also <span class="text-primary">Love</span></h2>
                        <p class="text-gray-500 text-lg mt-2 font-medium">Curated recommendations based on your current choice.</p>
                    </div>
                    <Button label="Explore All" severity="secondary" text class="font-bold" />
                </div>
                <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-8">
                    <Card v-for="sim in similarExamples" :key="sim.id" class="overflow-hidden border-none shadow-sm hover:shadow-xl transition-all cursor-pointer group rounded-[2.5rem] bg-white border border-gray-50" @click="router.push(`/Examples/${sim.id}`)">
                        <template #header>
                            <div class="aspect-square overflow-hidden bg-gray-50 m-3 rounded-[1.8rem]">
                                <img v-if="sim.image_url" :src="sim.image_url" :alt="sim.name" class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700" />
                                <div v-else class="w-full h-full flex items-center justify-center text-gray-200">
                                    <i class="pi pi-image text-3xl"></i>
                                </div>
                            </div>
                        </template>
                        <template #title>
                            <span class="text-base font-black line-clamp-1 group-hover:text-primary transition-colors block px-2">{{ sim.name }}</span>
                        </template>
                        <template #subtitle>
                            <span class="text-xl font-black text-gray-900 block px-2 pb-2">
                                {{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(sim.price) }}
                            </span>
                        </template>
                    </Card>
                </div>
            </div>
        </div>

        <div v-else class="text-center py-40 bg-gray-50 rounded-[4rem] border-2 border-dashed border-gray-200">
            <div class="bg-white w-24 h-24 rounded-full flex items-center justify-center mx-auto shadow-md mb-8">
                <i class="pi pi-exclamation-triangle text-4xl text-gray-300"></i>
            </div>
            <h2 class="text-4xl font-black text-gray-900 mb-4 tracking-tighter">Example Not Found</h2>
            <p class="text-gray-500 text-lg mb-12 max-w-md mx-auto">The item you're looking for might have been moved or is no longer available.</p>
            <Button label="Return to Shop" icon="pi pi-home" @click="router.push('/')" class="px-8 py-4 rounded-2xl font-bold shadow-xl shadow-primary/20" />
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