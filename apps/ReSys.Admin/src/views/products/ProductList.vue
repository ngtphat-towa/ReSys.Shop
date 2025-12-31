<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getProducts, deleteProduct, type Product } from '@/services/productService';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import { useRouter } from 'vue-router';
import { useConfirm } from "primevue/useconfirm";

const products = ref<Product[]>([]);
const loading = ref(true);
const router = useRouter();
const confirm = useConfirm();

const loadProducts = async () => {
    loading.value = true;
    try {
        products.value = await getProducts();
    } finally {
        loading.value = false;
    }
};

const editProduct = (id: string) => {
    router.push(`/products/edit/${id}`);
};

const confirmDelete = (product: Product) => {
    confirm.require({
        message: `Are you sure you want to delete ${product.name}?`,
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: async () => {
            await deleteProduct(product.id);
            await loadProducts();
        }
    });
};

onMounted(() => {
    loadProducts();
});
</script>

<template>
    <div class="card">
        <div class="flex justify-between items-center mb-4">
            <h1 class="text-2xl font-bold">Products</h1>
            <Button label="New Product" icon="pi pi-plus" @click="router.push('/products/create')" />
        </div>
        
        <DataTable :value="products" :loading="loading" tableStyle="min-width: 50rem">
            <Column field="name" header="Name"></Column>
            <Column field="price" header="Price">
                <template #body="slotProps">
                    {{ slotProps.data.price }}
                </template>
            </Column>
            <Column header="Image">
                <template #body="slotProps">
                    <img v-if="slotProps.data.imageUrl" :src="slotProps.data.imageUrl" :alt="slotProps.data.name" class="w-16 h-16 object-cover rounded shadow-sm" />
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" severity="info" text rounded aria-label="Edit" @click="editProduct(slotProps.data.id)" />
                    <Button icon="pi pi-trash" severity="danger" text rounded aria-label="Delete" @click="confirmDelete(slotProps.data)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
