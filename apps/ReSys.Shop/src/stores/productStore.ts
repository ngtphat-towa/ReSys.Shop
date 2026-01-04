import { defineStore } from 'pinia';
import { ref } from 'vue';
import { getProducts, getProductById, getSimilarProducts } from '@/services/productService';
import type { ProductListItem, ProductDetail, ProductQuery } from '@/types/product';

export const useProductStore = defineStore('product', () => {
    const products = ref<ProductListItem[]>([]);
    const currentProduct = ref<ProductDetail | null>(null);
    const similarProducts = ref<ProductListItem[]>([]);
    const loading = ref(false);
    const totalRecords = ref(0);

    async function fetchProducts(query?: ProductQuery) {
        loading.value = true;
        try {
            const response = await getProducts(query);
            products.value = response.data;
            totalRecords.value = response.meta?.total_count ?? 0;
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function fetchProductDetails(id: string) {
        loading.value = true;
        try {
            const [productRes, similarRes] = await Promise.all([
                getProductById(id),
                getSimilarProducts(id)
            ]);
            currentProduct.value = productRes.data;
            similarProducts.value = similarRes.data;
            return productRes;
        } finally {
            loading.value = false;
        }
    }

    return {
        products,
        currentProduct,
        similarProducts,
        loading,
        totalRecords,
        fetchProducts,
        fetchProductDetails
    };
});
