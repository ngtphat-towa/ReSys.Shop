<script setup lang="ts">
/**
 * Example Form View
 * Orchestrates creating and editing Example entities.
 * Uses VeeValidate + Zod for robust form state and validation.
 */
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useExampleStore } from '../example.store'
import { storeToRefs } from 'pinia'
import { showToast } from '@/shared/api/client'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { ExampleSchema } from '../example.validator'
import { exampleLocales } from '../example.locales'
import ExampleMediaUpload from '../components/media-upload.vue'
import AppBreadcrumb from '@/shared/components/breadcrumb.vue'

// --- ROUTING & STORE ---
const route = useRoute()
const router = useRouter()
const exampleStore = useExampleStore()
const { loading } = storeToRefs(exampleStore)

/** True if we are in "Edit" mode (id exists in route). */
const isEdit = computed(() => route.params.id !== undefined)
const ExampleId = route.params.id as string

// --- FORM INITIALIZATION (VeeValidate) ---
const { defineField, handleSubmit, errors, setValues, setErrors, values } = useForm({
  validationSchema: toTypedSchema(ExampleSchema),
  initialValues: {
    name: '',
    description: '',
    price: 0.01,
    image_url: null as string | null,
  },
})

/** Binding fields to the Zod schema. */
const [name] = defineField('name')
const [description] = defineField('description')
const [price] = defineField('price')

// --- MEDIA STATE ---
const imageFile = ref<File | null>(null)

/**
 * Handles the event from ExampleMediaUpload component.
 * Stores the file to be uploaded upon form submission.
 */
const handleFileChange = (file: File | null) => {
  imageFile.value = file
}

// --- CORE ACTIONS ---

/**
 * Loads the existing data if we are in edit mode.
 * Fetches the example details by ID from the store and populates the form fields.
 * If fetching fails, redirects the user back to the list view.
 */
const loadExample = async () => {
  if (!isEdit.value) return
  const result = await exampleStore.fetchExampleById(ExampleId)
  if (result.success) {
    const data = result.data
    // Update VeeValidate form state with server data
    setValues({
      name: data.name,
      description: data.description || '',
      price: data.price,
      image_url: data.image_url || null,
    })
  } else {
    showToast(
      'error',
      exampleLocales.common?.error || 'Error',
      exampleLocales.messages?.load_error || 'Failed to load item details.',
    )
    router.push({ name: 'testing.examples.list' })
  }
}

/**
 * Main form submission handler.
 * Decides between create and update based on current mode.
 *
 * Flow:
 * 1. Submit basic JSON data (Create or Update).
 * 2. If a new image file was selected, upload it in a second step via multipart/form-data.
 * 3. Handle successful redirect or backend validation error mapping.
 */
const onSubmit = handleSubmit(async (formValues) => {
  const result = isEdit.value
    ? await exampleStore.updateExample(ExampleId, { ...formValues, image_url: values.image_url || undefined }, imageFile.value || undefined)
    : await exampleStore.createExample(formValues, imageFile.value || undefined);

  if (result.success) {
    showToast(
      'success',
      exampleLocales.common?.success || 'Success',
      isEdit.value 
        ? (exampleLocales.messages?.update_success || 'Updated') 
        : (exampleLocales.messages?.create_success || 'Created')
    );
    router.push({ name: 'testing.examples.list' });
    return;
  }

  const apiError = result.error;
  if (!apiError) return;

  // 1. Handle Validation Errors (400)
  if (apiError.errors && Object.keys(apiError.errors).length > 0) {
    const formErrors: Record<string, string> = {};
    const unmappedMessages: string[] = [];
    const formFields = Object.keys(values);

    Object.entries(apiError.errors).forEach(([key, messages]) => {
      // Backend keys are snake_case (e.g., 'request.image_url')
      const normalizedKey = key.replace('request.', '').toLowerCase();
      
      // Look for a form field that matches or is part of the key
      const field = formFields.find(f => normalizedKey === f || normalizedKey.includes(f));

      if (field) {
        formErrors[field] = messages[0] || 'Invalid value';
      } else {
        unmappedMessages.push(...messages);
      }
    });

    setErrors(formErrors);

    // Show toast for validation errors that couldn't be bound to a specific input
    if (unmappedMessages.length > 0) {
      showToast('warn', apiError.title || 'Validation Error', unmappedMessages.join('. '));
    }
  } else {
    // 3. Handle Global Errors (409, 500, etc.)
    // We show a toast here as a safety measure in case the global client was silent.
    showToast(
      apiError.status && apiError.status < 500 ? 'warn' : 'error',
      apiError.title || exampleLocales.common?.error || 'Error',
      apiError.detail || 'An unexpected error occurred.'
    );
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
      <AppBreadcrumb :locales="exampleLocales" />

      <div class="flex flex-col justify-between gap-4 md:flex-row md:items-center">
        <div class="flex items-center gap-4">
          <Button
            type="button"
            icon="pi pi-arrow-left"
            text
            rounded
            severity="secondary"
            @click="router.push({ name: 'testing.examples.list' })"
            class="bg-surface-100 dark:bg-surface-800"
          />
          <div>
            <h1 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
              {{ isEdit ? exampleLocales.titles.edit : exampleLocales.titles.create }}
            </h1>
            <p class="text-sm text-surface-500 dark:text-surface-400">
              {{
                isEdit
                  ? `${exampleLocales.messages?.modifying || 'Modifying'}: ${name || exampleLocales.messages?.loading}`
                  : (exampleLocales.descriptions?.create || 'Fill in the details to add a new item.')
              }}
            </p>
          </div>
        </div>
        <div class="flex gap-2">
          <Button
            type="button"
            :label="exampleLocales.actions.cancel"
            severity="secondary"
            text
            @click="router.push({ name: 'testing.examples.list' })"
          />
          <Button
            type="submit"
            :label="isEdit ? exampleLocales.actions.save_edit : exampleLocales.actions.save_create"
            icon="pi pi-check"
            :loading="loading"
            class="px-6 shadow-lg rounded-xl"
          />
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-8 lg:grid-cols-3">
      <!-- Details -->
      <div class="space-y-6 lg:col-span-2">
        <Card
          class="overflow-hidden border-none shadow-sm rounded-2xl border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900"
        >
          <template #title>
            <span class="text-xl font-bold text-surface-800 dark:text-surface-50">{{
              exampleLocales.titles.basic_info
            }}</span>
          </template>
          <template #content>
            <div class="flex flex-col gap-6 pt-2">
              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                  <label for="name" class="font-bold text-surface-700 dark:text-surface-200">{{
                    exampleLocales.labels?.name
                  }}</label>
                  <i
                    class="pi pi-info-circle text-surface-400 cursor-help"
                    v-tooltip="exampleLocales.tooltips?.name"
                  ></i>
                </div>
                <InputText
                  id="name"
                  v-model="name"
                  class="w-full"
                  :invalid="!!errors.name"
                  :placeholder="exampleLocales.placeholders?.name"
                />
                <small v-if="errors.name" class="font-medium text-red-500">{{ errors.name }}</small>
              </div>

              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                  <label
                    for="description"
                    class="font-bold text-surface-700 dark:text-surface-200"
                    >{{ exampleLocales.labels?.description }}</label
                  >
                  <i
                    class="pi pi-info-circle text-surface-400 cursor-help"
                    v-tooltip="exampleLocales.tooltips?.description"
                  ></i>
                </div>
                <Textarea
                  id="description"
                  v-model="description"
                  rows="6"
                  class="w-full"
                  :invalid="!!errors.description"
                  :placeholder="exampleLocales.placeholders?.description"
                />
                <small v-if="errors.description" class="font-medium text-red-500">{{
                  errors.description
                }}</small>
              </div>

              <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div class="flex flex-col gap-2">
                  <div class="flex items-center gap-2">
                    <label for="price" class="font-bold text-surface-700 dark:text-surface-200">{{
                      exampleLocales.labels?.price
                    }}</label>
                    <i
                      class="pi pi-info-circle text-surface-400 cursor-help"
                      v-tooltip="exampleLocales.tooltips?.price"
                    ></i>
                  </div>
                  <InputNumber
                    id="price"
                    v-model="price"
                    mode="currency"
                    currency="USD"
                    locale="en-US"
                    class="w-full"
                    :invalid="!!errors.price"
                  />
                  <small v-if="errors.price" class="font-medium text-red-500">{{
                    errors.price
                  }}</small>
                </div>
                <div class="flex flex-col gap-2">
                  <label class="font-bold text-surface-700 dark:text-surface-200">{{
                    exampleLocales.labels?.category
                  }}</label>
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

      <!-- Media -->
      <div class="space-y-6 lg:col-span-1">
        <ExampleMediaUpload
          :modelValue="values.image_url || null"
          @update:modelValue="(val) => setValues({ ...values, image_url: val })"
          :locales="exampleLocales"
          @file-change="handleFileChange"
        />

        <!-- Summary -->
        <Card
          class="overflow-hidden border border-none shadow-sm rounded-2xl border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900"
        >
          <template #title>
            <span class="text-lg font-bold text-surface-800 dark:text-surface-50">{{
              exampleLocales.labels?.summary
            }}</span>
          </template>
          <template #content>
            <div class="flex flex-col gap-4">
              <div class="flex items-center justify-between text-sm">
                <span class="font-medium text-surface-500">{{
                  exampleLocales.labels?.status
                }}</span>
                <Badge
                  :value="isEdit ? exampleLocales.table?.active : exampleLocales.table?.draft"
                  :severity="isEdit ? 'success' : 'warning'"
                ></Badge>
              </div>
              <Divider class="my-0" />
              <div class="flex items-center justify-between">
                <span class="font-medium text-surface-500">{{
                  exampleLocales.labels?.live_price
                }}</span>
                <span class="text-xl font-black text-primary">{{
                  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                    price || 0,
                  )
                }}</span>
              </div>
            </div>
          </template>
        </Card>
      </div>
    </div>
  </form>
</template>

<style scoped>
:deep(.p-card-body) {
  padding: 2rem;
}
</style>
