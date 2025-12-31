<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getProducts, type Product } from '@/services/productService';
import Card from 'primevue/card';
import Button from 'primevue/button';

const products = ref<Product[]>([]);
const loading = ref(true);

onMounted(async () => {
  try {
    products.value = await getProducts();
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div v-if="loading" class="text-center">Loading products...</div>
  <div v-else class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-6">
    <Card v-for="product in products" :key="product.id" class="overflow-hidden">
        <template #header>
            <img v-if="product.image_url" :src="product.image_url" :alt="product.name" class="w-full h-48 object-cover" />
            <div v-else class="w-full h-48 bg-gray-200 flex items-center justify-center text-gray-400">
                No Image
            </div>
        </template>
        <template #title>{{ product.name }}</template>
        <template #subtitle>${{ product.price }}</template>
        <template #content>
            <p class="truncate">{{ product.description }}</p>
        </template>
        <template #footer>
            <div class="flex gap-2 mt-1">
                <Button label="View" severity="secondary" outlined class="w-full" />
                <Button label="Add to Cart" class="w-full" />
            </div>
        </template>
    </Card>
  </div>
</template>
