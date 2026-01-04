<script setup lang="ts">
import { ref, onMounted, computed, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useExampleStore } from '../example.store';
import { storeToRefs } from 'pinia';
import { showToast } from '@/shared/api/client';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { ExampleSchema } from '../example.validator';
import { exampleLocales } from '../example.locales';
import ExampleMediaUpload from '../components/ExampleMediaUpload.vue';

const route = useRoute();
const router = useRouter()
const exampleStore = useExampleStore()
const { loading } = storeToRefs(exampleStore)

const isEdit = computed(() => route.params.id !== undefined)
const ExampleId = route.params.id as string

// 1. Initialize VeeValidate Form
const { defineField, handleSubmit, errors, setValues, setErrors, values } = useForm({
  validationSchema: toTypedSchema(ExampleSchema),
  initialValues: {
    name: '',
    description: '',
    price: 0.01,
    image_url: null as string | null
  }
});

const [name] = defineField('name');
const [description] = defineField('description');
const [price] = defineField('price');

// --- MEDIA STATE ---
const imageFile = ref<File | null>(null);
const handleFileChange = (file: File | null) => {
    imageFile.value = file;
};

const loadExample = async () => {
  if (!isEdit.value) return
  const result = await exampleStore.fetchExampleById(ExampleId)
  if (result.success) {
    const data = result.data.data
    setValues({
        name: data.name,
        description: data.description,
        price: data.price,
        image_url: data.image_url
    });
  } else {
    showToast('error', 'Error', 'Failed to load item details.');
    router.push('/Examples')
  }
}

const onSubmit = handleSubmit(async (formValues) => {
    let result;
    if (isEdit.value) {
      result = await exampleStore.updateExample(
        ExampleId,
        { ...formValues, image_url: values.image_url || undefined },
        imageFile.value || undefined
      )
    } else {
      result = await exampleStore.createExample(formValues, imageFile.value || undefined)
    }

    if (result.success) {
        showToast('success', 'Success', isEdit.value ? (exampleLocales.messages?.update_success || 'Updated') : (exampleLocales.messages?.create_success || 'Created'));
        router.push('/Examples');
    } else if (result.error?.errors) {
        const apiErrors = result.error.errors;
        const formErrors: Record<string, string> = {};
        Object.keys(apiErrors).forEach(key => {
            const field = key.replace('request.', '');
            formErrors[field] = apiErrors[key][0] || 'Invalid field';
        });
        setErrors(formErrors);
        showToast('warn', 'Validation Failed', exampleLocales.messages?.validation_failed || 'Please check the form.');
    }
});

onMounted(() => {
  loadExample()
})
</script>

<template>
    <form @submit="onSubmit" class="p-6 mx-auto">
        <!-- Header -->
        <div class="mb-6">
            <nav class="flex mb-4 text-sm" aria-label="Breadcrumb">
                <ol class="inline-flex items-center space-x-1 md:space-x-3">
                    <li class="inline-flex items-center">
                        <router-link to="/" class="transition-colors text-surface-500 hover:text-primary dark:text-surface-400">
                            <i class="mr-2 pi pi-home"></i> {{ exampleLocales.titles.breadcrumb_home }}
                        </router-link>
                    </li>
                    <li>
                        <div class="flex items-center">
                            <i class="pi pi-chevron-right text-surface-400 mx-2 text-[10px]"></i>
                            <router-link to="/Examples" class="transition-colors text-surface-500 hover:text-primary dark:text-surface-400">
                                {{ exampleLocales.titles.breadcrumb_parent }}
                            </router-link>
                        </div>
                    </li>
                    <li aria-current="page">
                        <div class="flex items-center">
                            <i class="pi pi-chevron-right text-surface-400 mx-2 text-[10px]"></i>
                            <span class="font-bold text-primary">{{ isEdit ? 'Edit' : 'Create' }}</span>
                        </div>
                    </li>
                </ol>
            </nav>

            <div class="flex flex-col justify-between gap-4 md:flex-row md:items-center">
                <div class="flex items-center gap-4">
                    <Button type="button" icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.push('/Examples')" class="bg-surface-100 dark:bg-surface-800" />
                    <div>
                        <h1 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-0">
                            {{ isEdit ? exampleLocales.titles.edit : exampleLocales.titles.create }}
                        </h1>
                        <p class="text-sm text-surface-500 dark:text-surface-400">
                            {{ isEdit ? `Modifying: ${name || exampleLocales.messages?.loading}` : 'Fill in the details to add a new item.' }}
                        </p>
                    </div>
                </div>
                <div class="flex gap-2">
                    <Button type="button" :label="exampleLocales.actions.cancel" severity="secondary" text @click="router.push('/Examples')" />
                    <Button type="submit" :label="isEdit ? exampleLocales.actions.save_edit : exampleLocales.actions.save_create" icon="pi pi-check" :loading="loading" class="px-6 shadow-lg rounded-xl" />
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 gap-8 lg:grid-cols-3">
            <!-- Details -->
            <div class="space-y-6 lg:col-span-2">
                <Card class="overflow-hidden border-none shadow-sm rounded-2xl border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900">
                    <template #title>
                        <span class="text-xl font-bold text-surface-800 dark:text-surface-50">Basic Information</span>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-6 pt-2">
                            <div class="flex flex-col gap-2">
                                <div class="flex items-center gap-2">
                                    <label for="name" class="font-bold text-surface-700 dark:text-surface-200">{{ exampleLocales.labels?.name }}</label>
                                    <i class="pi pi-info-circle text-surface-400 cursor-help" v-tooltip="exampleLocales.tooltips?.name"></i>
                                </div>
                                <InputText id="name" v-model="name" class="w-full" :invalid="!!errors.name" :placeholder="exampleLocales.placeholders?.name" />
                                <small v-if="errors.name" class="font-medium text-red-500">{{ errors.name }}</small>
                            </div>

                            <div class="flex flex-col gap-2">
                                <div class="flex items-center gap-2">
                                    <label for="description" class="font-bold text-surface-700 dark:text-surface-200">{{ exampleLocales.labels?.description }}</label>
                                    <i class="pi pi-info-circle text-surface-400 cursor-help" v-tooltip="exampleLocales.tooltips?.description"></i>
                                </div>
                                <Textarea id="description" v-model="description" rows="6" class="w-full" :invalid="!!errors.description" :placeholder="exampleLocales.placeholders?.description" />
                                <small v-if="errors.description" class="font-medium text-red-500">{{ errors.description }}</small>
                            </div>

                            <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                                <div class="flex flex-col gap-2">
                                    <div class="flex items-center gap-2">
                                        <label for="price" class="font-bold text-surface-700 dark:text-surface-200">{{ exampleLocales.labels?.price }}</label>
                                        <i class="pi pi-info-circle text-surface-400 cursor-help" v-tooltip="exampleLocales.tooltips?.price"></i>
                                    </div>
                                    <InputNumber id="price" v-model="price" mode="currency" currency="USD" locale="en-US" class="w-full" :invalid="!!errors.price" />
                                    <small v-if="errors.price" class="font-medium text-red-500">{{ errors.price }}</small>
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-surface-700 dark:text-surface-200">{{ exampleLocales.labels?.category }}</label>
                                    <InputText disabled value="Default Category" class="bg-surface-50 dark:bg-surface-800 opacity-60" />
                                </div>
                            </div>
                        </div>
                    </template>
                </Card>
            </div>

            <!-- Media -->
            <div class="space-y-6 lg:col-span-1">
                <ExampleMediaUpload
                    :modelValue="values.image_url"
                    @update:modelValue="val => setValues({ ...values, image_url: val })"
                    :locales="exampleLocales"
                    @file-change="handleFileChange"
                />

                <!-- Summary -->
                <Card class="overflow-hidden border border-none shadow-sm rounded-2xl border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900">
                    <template #title>
                        <span class="text-lg font-bold text-surface-800 dark:text-surface-50">{{ exampleLocales.labels?.summary }}</span>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-4">
                            <div class="flex items-center justify-between text-sm">
                                <span class="font-medium text-surface-500">Status</span>
                                <Badge :value="isEdit ? exampleLocales.table?.active : 'Draft'" :severity="isEdit ? 'success' : 'warning'"></Badge>
                            </div>
                            <Divider class="my-0" />
                            <div class="flex items-center justify-between">
                                <span class="font-medium text-surface-500">Live Price</span>
                                <span class="text-xl font-black text-primary">{{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price || 0) }}</span>
                            </div>
                        </div>
                    </template>
                </Card>
            </div>
        </div>
    </form>
</template>

<style scoped>
:deep(.p-card-body) { padding: 2rem; }
</style>
