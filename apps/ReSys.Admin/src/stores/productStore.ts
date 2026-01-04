import { defineStore } from 'pinia';
import { ref } from 'vue';
import { 
    getProducts, 
    getProductById, 
    createProduct as apiCreateProduct, 
    updateProduct as apiUpdateProduct, 
    deleteProduct as apiDeleteProduct,
    updateProductImage as apiUpdateProductImage
} from '@/services/productService';
import type { ProductListItem, ProductDetail, ProductQuery, CreateProductRequest, UpdateProductRequest } from '@/types/product';
import type { ApiResponse } from '@/types/api';

export const useProductStore = defineStore('product', () => {
    const products = ref<ProductListItem[]>([]);
    const currentProduct = ref<ProductDetail | null>(null);
    const loading = ref(false);
    const totalRecords = ref(0);
    const pagination = ref({
        page: 1,
        pageSize: 10
    });

    async function fetchProducts(query?: ProductQuery) {
        loading.value = true;
        try {
            const response = await getProducts({
                page: pagination.value.page,
                page_size: pagination.value.pageSize,
                ...query
            });
            products.value = response.data;
            totalRecords.value = response.meta?.total_count ?? 0;
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function fetchProductById(id: string) {
        loading.value = true;
        try {
            const response = await getProductById(id);
            currentProduct.value = response.data;
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function createProduct(request: CreateProductRequest, image?: File) {
        loading.value = true;
        try {
            const response = await apiCreateProduct(request);
            if (image && response.data.id) {
                await apiUpdateProductImage(response.data.id, image);
            }
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function updateProduct(id: string, request: UpdateProductRequest, image?: File) {
        loading.value = true;
        try {
            const response = await apiUpdateProduct(id, request);
            if (image) {
                await apiUpdateProductImage(id, image);
            }
            return response;
        } finally {
            loading.value = false;
        }
    }

    async function deleteProduct(id: string) {
        loading.value = true;
        try {
            await apiDeleteProduct(id);
            await fetchProducts();
        } finally {
            loading.value = false;
        }
    }

    return {
        products,
        currentProduct,
        loading,
        totalRecords,
        pagination,
        fetchProducts,
        fetchProductById,
        createProduct,
        updateProduct,
        deleteProduct
    };
});
