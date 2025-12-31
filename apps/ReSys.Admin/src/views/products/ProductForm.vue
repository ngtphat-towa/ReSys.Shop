<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { createProduct, updateProduct, updateProductImage, getProductById } from '@/services/productService';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import FileUpload from 'primevue/fileupload';
import { showToast } from '@/services/apiClient';

const route = useRoute();
const router = useRouter();

const isEdit = computed(() => route.params.id !== undefined);
const productId = route.params.id as string;

const name = ref('');
const description = ref('');
const price = ref<number | null>(null);
const imageFile = ref<File | null>(null);
const currentImageUrl = ref('');

const loading = ref(false);

const onFileSelect = (event: any) => {
    imageFile.value = event.files[0];
};

const loadProduct = async () => {
    if (!isEdit.value) return;
    loading.value = true;
    try {
        const product = await getProductById(productId);
        name.value = product.name;
        description.value = product.description;
        price.value = product.price;
        currentImageUrl.value = product.imageUrl;
    } catch (e) {
        // Handled by interceptor
    } finally {
        loading.value = false;
    }
};

const saveProduct = async () => {
    if (!name.value || !price.value) {
        showToast('warn', 'Validation', 'Name and Price are required.');
        return;
    }

    loading.value = true;
    try {
        if (isEdit.value) {
            await updateProduct(productId, {
                name: name.value,
                description: description.value,
                price: price.value
            });

            if (imageFile.value) {
                await updateProductImage(productId, imageFile.value);
            }

            showToast('success', 'Success', 'Product updated successfully');
        } else {
            const formData = new FormData();
            formData.append('name', name.value);
            formData.append('description', description.value);
            formData.append('price', price.value.toString());
            if (imageFile.value) {
                formData.append('image', imageFile.value);
            }
            await createProduct(formData);
            showToast('success', 'Success', 'Product created successfully');
        }
        router.push('/products');
    } catch (e) {
        // Handled by interceptor
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    loadProduct();
});
</script>

<template>
    <div class="card max-w-2xl mx-auto">
        <h2 class="text-2xl font-bold mb-4">{{ isEdit ? 'Edit Product' : 'New Product' }}</h2>
        
        <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-2">
                <label for="name">Name</label>
                <InputText id="name" v-model="name" />
            </div>

            <div class="flex flex-col gap-2">
                <label for="description">Description</label>
                <Textarea id="description" v-model="description" rows="5" />
            </div>

            <div class="flex flex-col gap-2">
                <label for="price">Price</label>
                <InputNumber id="price" v-model="price" mode="currency" currency="USD" locale="en-US" />
            </div>

            <div class="flex flex-col gap-2">
                <label>Image</label>
                <div v-if="currentImageUrl && !imageFile" class="mb-2">
                    <img :src="currentImageUrl" class="w-32 h-32 object-cover rounded" />
                </div>
                <FileUpload mode="basic" name="image" accept="image/*" :maxFileSize="1000000" @select="onFileSelect" :auto="false" chooseLabel="Choose Image" />
            </div>

            <div class="flex gap-2 mt-4">
                <Button label="Save" icon="pi pi-check" @click="saveProduct" :loading="loading" />
                <Button label="Cancel" severity="secondary" @click="router.push('/products')" />
            </div>
        </div>
    </div>
</template>
