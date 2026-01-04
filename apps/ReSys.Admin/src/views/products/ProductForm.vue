<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useProductStore } from '@/stores/productStore';
import { storeToRefs } from 'pinia';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputNumber from 'primevue/inputnumber';
import FileUpload from 'primevue/fileupload';
import Card from 'primevue/card';
import Divider from 'primevue/divider';
import { showToast } from '@/services/apiClient';

const route = useRoute();
const router = useRouter();
const productStore = useProductStore();
const { loading } = storeToRefs(productStore);

const isEdit = computed(() => route.params.id !== undefined);
const productId = route.params.id as string;

const form = ref({
    name: '',
    description: '',
    price: 0 as number | null,
});
const imageFile = ref<File | null>(null);
const currentImageUrl = ref('');

const onFileSelect = (event: any) => {
    imageFile.value = event.files[0];
};

const loadProduct = async () => {
    if (!isEdit.value) return;
    try {
        const response = await productStore.fetchProductById(productId);
        const data = response.data;
        form.value.name = data.name;
        form.value.description = data.description;
        form.value.price = data.price;
        currentImageUrl.value = data.image_url;
    } catch (e) {
        router.push('/products');
    }
};

const saveProduct = async () => {
    if (!form.value.name || form.value.price === null) {
        showToast('warn', 'Validation', 'Name and Price are required.');
        return;
    }

    try {
        if (isEdit.value) {
            await productStore.updateProduct(productId, {
                name: form.value.name,
                description: form.value.description,
                price: form.value.price
            }, imageFile.value || undefined);
            showToast('success', 'Updated', 'Product has been successfully updated.');
        } else {
            await productStore.createProduct({
                name: form.value.name,
                description: form.value.description,
                price: form.value.price ?? 0
            }, imageFile.value || undefined);
            showToast('success', 'Created', 'New product has been successfully added.');
        }
        router.push('/products');
    } catch (e) {
        // Handled by store/apiClient
    }
};

onMounted(() => {
    loadProduct();
});
</script>

<template>
    <div class="p-6 max-w-4xl mx-auto">
        <div class="flex items-center gap-4 mb-8">
            <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.push('/products')" />
            <div>
                <h1 class="text-3xl font-black text-gray-900 tracking-tight">
                    {{ isEdit ? 'Edit Product' : 'Add New Product' }}
                </h1>
                <p class="text-gray-500 mt-1">
                    {{ isEdit ? 'Update existing product information and images.' : 'Create a new item in your product catalog.' }}
                </p>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div class="lg:col-span-2">
                <Card class="border-none shadow-sm rounded-2xl overflow-hidden border border-gray-100">
                    <template #title>
                        <span class="text-xl font-bold text-gray-800">Basic Information</span>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-6 pt-2">
                            <div class="flex flex-col gap-2">
                                <label for="name" class="font-bold text-gray-700">Product Name</label>
                                <InputText id="name" v-model="form.name" class="w-full" placeholder="e.g. Premium Wireless Headphones" />
                            </div>

                            <div class="flex flex-col gap-2">
                                <label for="description" class="font-bold text-gray-700">Description</label>
                                <Textarea id="description" v-model="form.description" rows="6" class="w-full" placeholder="Provide a detailed description of the product features..." />
                            </div>

                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div class="flex flex-col gap-2">
                                    <label for="price" class="font-bold text-gray-700">Price (USD)</label>
                                    <InputNumber id="price" v-model="form.price" mode="currency" currency="USD" locale="en-US" class="w-full" />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-gray-700">Category</label>
                                    <InputText disabled value="Default Category" class="bg-gray-50 opacity-60" />
                                </div>
                            </div>
                        </div>
                    </template>
                </Card>
            </div>

            <div class="lg:col-span-1">
                <Card class="border-none shadow-sm rounded-2xl overflow-hidden border border-gray-100 mb-8">
                    <template #title>
                        <span class="text-xl font-bold text-gray-800">Media</span>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-6 pt-2">
                            <div v-if="currentImageUrl && !imageFile" class="relative group rounded-xl overflow-hidden aspect-square border-2 border-dashed border-gray-200 p-2">
                                <img :src="currentImageUrl" class="w-full h-full object-cover rounded-lg" />
                                <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center text-white text-sm font-medium">
                                    Current Product Image
                                </div>
                            </div>
                            <div v-else-if="imageFile" class="relative rounded-xl overflow-hidden aspect-square border-2 border-primary/20 border-dashed p-2 bg-primary/5">
                                <div class="flex flex-col items-center justify-center h-full text-primary">
                                    <i class="pi pi-file-image text-4xl mb-2"></i>
                                    <span class="text-xs font-bold truncate max-w-full px-4">{{ imageFile.name }}</span>
                                    <span class="text-[10px] uppercase mt-1">Selected to upload</span>
                                </div>
                            </div>
                            <div v-else class="rounded-xl aspect-square border-2 border-dashed border-gray-200 flex flex-col items-center justify-center bg-gray-50 text-gray-400">
                                <i class="pi pi-image text-5xl mb-3 opacity-20"></i>
                                <span class="text-sm font-medium">No image selected</span>
                            </div>

                            <FileUpload 
                                mode="basic" 
                                name="image" 
                                accept="image/*" 
                                :maxFileSize="1000000" 
                                @select="onFileSelect" 
                                :auto="false" 
                                chooseLabel="Change Image"
                                class="w-full p-button-outlined"
                            />
                        </div>
                    </template>
                </Card>

                <div class="flex flex-col gap-3">
                    <Button label="Save Changes" icon="pi pi-check" @click="saveProduct" :loading="loading" class="w-full py-4 text-lg font-bold shadow-xl" />
                    <Button label="Discard Changes" severity="secondary" outlined @click="router.push('/products')" class="w-full" />
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
:deep(.p-card-body) {
    padding: 2rem;
}
</style>
