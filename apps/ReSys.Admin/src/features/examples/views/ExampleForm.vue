<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useExampleStore } from '../example.store';
import { storeToRefs } from 'pinia';
import { showToast } from '@/shared/api/api-client';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { ExampleSchema } from '../example.validator';
import { ExampleLocales } from '../example.locales';

const route = useRoute();
const router = useRouter()
const exampleStore = useExampleStore()
const { loading } = storeToRefs(exampleStore)

const isEdit = computed(() => route.params.id !== undefined)
const ExampleId = route.params.id as string

// 1. Initialize VeeValidate Form with Zod Schema
const { defineField, handleSubmit, errors, setValues, setErrors } = useForm({
  validationSchema: toTypedSchema(ExampleSchema),
  initialValues: {
    name: '',
    description: '',
    price: 0.01
  }
});

const [name, nameProps] = defineField('name');
const [description, descriptionProps] = defineField('description');
const [price, priceProps] = defineField('price');

const imageFile = ref<File | null>(null)
const currentImageUrl = ref('')

const onFileSelect = (event: any) => {
  imageFile.value = event.files[0]
}

const loadExample = async () => {
  if (!isEdit.value) return
  try {
    const response = await exampleStore.fetchExampleById(ExampleId)
    const data = response.data
    setValues({
        name: data.name,
        description: data.description,
        price: data.price
    });
    currentImageUrl.value = data.image_url || '';
  } catch (e) {
    router.push('/Examples')
  }
}

const saveExample = handleSubmit(async (values) => {
  try {
    if (isEdit.value) {
      await exampleStore.updateExample(
        ExampleId,
        {
          ...values,
          image_url: currentImageUrl.value,
        },
        imageFile.value || undefined,
      )
      showToast('success', 'Updated', ExampleLocales.messages.update_success)
    } else {
      await exampleStore.createExample(
        values,
        imageFile.value || undefined,
      )
      showToast('success', 'Created', ExampleLocales.messages.create_success)
    }
    router.push('/Examples')
  } catch (e: any) {
    if (e.response?.data?.errors) {
        const apiErrors = e.response.data.errors;
        const formErrors: Record<string, string> = {};
        
        Object.keys(apiErrors).forEach(key => {
            const field = key.replace('request.', '');
            formErrors[field] = apiErrors[key][0];
        });
        
        setErrors(formErrors);
    }
  }
});

onMounted(() => {
  loadExample()
})
</script>

<template>
    <div class="p-6 mx-auto">
        <div class="mb-6">
            <nav class="flex mb-4" aria-label="Breadcrumb">
                <ol class="inline-flex items-center space-x-1 md:space-x-3">
                    <li class="inline-flex items-center">
                        <router-link to="/" class="text-sm font-medium text-surface-500 hover:text-primary dark:text-surface-400">
                            <i class="pi pi-home mr-2"></i> {{ ExampleLocales.titles.breadcrumb_home }}
                        </router-link>
                    </li>
                    <li>
                        <div class="flex items-center">
                            <i class="pi pi-chevron-right text-surface-400 mx-2 text-xs"></i>
                            <router-link to="/Examples" class="text-sm font-medium text-surface-500 hover:text-primary dark:text-surface-400">{{ ExampleLocales.titles.breadcrumb_parent }}</router-link>
                        </div>
                    </li>
                    <li aria-current="page">
                        <div class="flex items-center">
                            <i class="pi pi-chevron-right text-surface-400 mx-2 text-xs"></i>
                            <span class="text-sm font-bold text-primary">{{ isEdit ? 'Edit' : 'Create' }}</span>
                        </div>
                    </li>
                </ol>
            </nav>
            
            <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div class="flex items-center gap-4">
                    <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.push('/Examples')" class="bg-surface-100 dark:bg-surface-800" />
                    <div>
                        <h1 class="text-3xl font-black text-surface-900 dark:text-surface-0 tracking-tight">
                            {{ isEdit ? ExampleLocales.titles.edit : ExampleLocales.titles.create }}
                        </h1>
                        <p class="text-surface-500 dark:text-surface-400 text-sm">
                            {{ isEdit ? `Modifying: ${name || ExampleLocales.messages.loading}` : 'Fill in the details to add a new item to your catalog.' }}
                        </p>
                    </div>
                </div>
                <div class="flex gap-2">
                    <Button :label="ExampleLocales.actions.cancel" severity="secondary" text @click="router.push('/Examples')" />
                    <Button :label="isEdit ? ExampleLocales.actions.save_edit : ExampleLocales.actions.save_create" icon="pi pi-check" @click="saveExample" :loading="loading" class="rounded-xl px-6" />
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2">
        <Card
          class="overflow-hidden border border-none shadow-sm rounded-2xl border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900"
        >
          <template #title>
            <span class="text-xl font-bold text-surface-800 dark:text-surface-50"
              >Basic Information</span
            >
          </template>
          <template #content>
            <div class="flex flex-col gap-6 pt-2">
              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                    <label for="name" class="font-bold text-surface-700 dark:text-surface-200">{{ ExampleLocales.labels.name }}</label>
                    <i class="pi pi-info-circle text-surface-400 cursor-help" v-tooltip="ExampleLocales.tooltips.name"></i>
                </div>
                <InputText
                  id="name"
                  v-model="name"
                  v-bind="nameProps"
                  class="w-full"
                  :invalid="!!errors.name"
                  :placeholder="ExampleLocales.placeholders.name"
                />
                <small v-if="errors.name" class="text-red-500 font-medium ml-1">
                    {{ errors.name }}
                </small>
              </div>

              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                    <label for="description" class="font-bold text-surface-700 dark:text-surface-200">{{ ExampleLocales.labels.description }}</label>
                    <i class="pi pi-info-circle text-surface-400 cursor-help" v-tooltip="ExampleLocales.tooltips.description"></i>
                </div>
                <Textarea
                  id="description"
                  v-model="description"
                  v-bind="descriptionProps"
                  rows="6"
                  class="w-full"
                  :invalid="!!errors.description"
                  :placeholder="ExampleLocales.placeholders.description"
                />
                <small v-if="errors.description" class="text-red-500 font-medium ml-1">
                    {{ errors.description }}
                </small>
              </div>

              <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div class="flex flex-col gap-2">
                  <div class="flex items-center gap-2">
                    <label for="price" class="font-bold text-surface-700 dark:text-surface-200">{{ ExampleLocales.labels.price }}</label>
                    <i class="pi pi-info-circle text-surface-400 cursor-help" v-tooltip="ExampleLocales.tooltips.price"></i>
                  </div>
                  <InputNumber
                    id="price"
                    v-model="price"
                    v-bind="priceProps"
                    mode="currency"
                    currency="USD"
                    locale="en-US"
                    class="w-full"
                    :invalid="!!errors.price"
                  />
                  <small v-if="errors.price" class="text-red-500 font-medium ml-1">
                    {{ errors.price }}
                  </small>
                </div>
                <div class="flex flex-col gap-2">
                  <label class="font-bold text-surface-700 dark:text-surface-200">{{ ExampleLocales.labels.category }}</label>
                  <InputText
                    disabled
                    value="Default Category"
                    class="bg-surface-50 dark:bg-surface-800 opacity-60"
                  />
                </div>
              </div>
            </div>
          </template>
        </Card>
      </div>

            <div class="lg:col-span-1 space-y-6">
                <Card class="border-none shadow-sm rounded-2xl overflow-hidden border border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900">
                    <template #title>
                        <div class="flex items-center gap-2">
                            <i class="pi pi-image text-primary"></i>
                            <span class="text-xl font-bold text-surface-800 dark:text-surface-50">{{ ExampleLocales.labels.media }}</span>
                            <i class="pi pi-info-circle text-surface-400 cursor-help text-sm" v-tooltip="ExampleLocales.tooltips.media"></i>
                        </div>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-6 pt-2">
                            <div v-if="currentImageUrl && !imageFile" class="relative group rounded-xl overflow-hidden aspect-square border-2 border-dashed border-surface-200 dark:border-surface-700 p-1 bg-surface-50 dark:bg-surface-800">
                                <img :src="currentImageUrl" class="w-full h-full object-cover rounded-lg" />
                                <div class="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center justify-center text-white p-4 text-center">
                                    <i class="pi pi-eye text-2xl mb-2"></i>
                                    <span class="text-xs font-bold">Current Live Image</span>
                                </div>
                            </div>
                            <div v-else-if="imageFile" class="relative rounded-xl overflow-hidden aspect-square border-2 border-primary/30 border-dashed p-4 bg-primary/5">
                                <div class="flex flex-col items-center justify-center h-full text-primary">
                                    <i class="pi pi-cloud-upload text-5xl mb-3"></i>
                                    <span class="text-sm font-bold truncate max-w-full px-4 text-surface-900 dark:text-surface-0">{{ imageFile.name }}</span>
                                    <span class="text-[10px] uppercase mt-2 tracking-widest font-black">Ready to upload</span>
                                </div>
                            </div>
                            <div v-else class="rounded-xl aspect-square border-2 border-dashed border-surface-200 dark:border-surface-700 flex flex-col items-center justify-center bg-surface-50 dark:bg-surface-800 text-surface-400">
                                <i class="pi pi-image text-5xl mb-3 opacity-20"></i>
                                <span class="text-sm font-medium">{{ ExampleLocales.messages.no_image }}</span>
                            </div>

                            <FileUpload 
                                mode="basic" 
                                name="image" 
                                accept="image/*" 
                                :maxFileSize="1000000" 
                                @select="onFileSelect" 
                                :auto="false" 
                                :chooseLabel="ExampleLocales.placeholders.upload"
                                class="w-full p-button-outlined rounded-xl"
                            />
                        </div>
                    </template>
                </Card>

                <Card class="border-none shadow-sm rounded-2xl overflow-hidden border border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900">
                    <template #title>
                        <span class="text-lg font-bold text-surface-800 dark:text-surface-50">{{ ExampleLocales.labels.summary }}</span>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-4">
                            <div class="flex justify-between items-center text-sm">
                                <span class="text-surface-500">Status</span>
                                <Badge :value="isEdit ? 'Active' : 'Draft'" :severity="isEdit ? 'success' : 'warning'"></Badge>
                            </div>
                            <div class="flex justify-between items-center text-sm">
                                <span class="text-surface-500">Visibility</span>
                                <span class="font-medium">Public</span>
                            </div>
                            <Divider class="my-0" />
                            <div class="flex justify-between items-center">
                                <span class="text-surface-500 font-medium">Final Price</span>
                                <span class="text-xl font-black text-primary">{{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price || 0) }}</span>
                            </div>
                        </div>
                    </template>
                </Card>
            </div>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-card-body) {
  padding: 2rem;
}
</style>