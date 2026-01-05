<script setup lang="ts">
/**
 * Example Form View
 * Orchestrates creating and editing Example entities.
 * Uses VeeValidate + Zod for robust form state and validation.
 */
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useExampleStore } from '../example.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { ExampleSchema } from '../example.schema'
import { ExampleStatus, STATUS_COLORS } from '../example.types'
import { exampleLocales } from '../example.locales'
import { useFormatter } from '@/shared/composables/formatter.use'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import ExampleMediaUpload from '../components/media-upload.component.vue'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'

// --- ROUTING & STORE ---
const route = useRoute()
const router = useRouter()
const exampleStore = useExampleStore()
const { loading } = storeToRefs(exampleStore)
const { formatCurrency } = useFormatter()
const { handleApiResult } = useApiErrorHandler()

/** True if we are in "Edit" mode (id exists in route). */
const isEdit = computed(() => route.params.id !== undefined)
const ExampleId = computed(() => route.params.id as string)

// --- FORM INITIALIZATION (VeeValidate) ---
const { defineField, handleSubmit, errors, setValues, setErrors, values, resetForm } = useForm({
  validationSchema: toTypedSchema(ExampleSchema),
  initialValues: {
    name: '',
    description: '',
    price: 0.01,
    image_url: null as string | null,
    status: ExampleStatus.Draft,
    hex_color: '#3B82F6',
  },
})

/** Binding fields to the Zod schema. */
const [name] = defineField('name')
const [description] = defineField('description')
const [price] = defineField('price')
const [status] = defineField('status')
const [hex_color] = defineField('hex_color')

const statusOptions = computed(() => [
  { label: exampleLocales.labels?.status_draft, value: ExampleStatus.Draft, icon: 'pi pi-pencil' },
  { label: exampleLocales.labels?.status_active, value: ExampleStatus.Active, icon: 'pi pi-check' },
  { label: exampleLocales.labels?.status_archived, value: ExampleStatus.Archived, icon: 'pi pi-box' },
])

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
  if (!isEdit.value) {
    exampleStore.clearCurrent()
    resetForm()
    return
  }

  const result = await exampleStore.fetchExampleById(ExampleId.value)
  const handled = handleApiResult(result, {
    errorTitle: exampleLocales.common?.error,
    genericError: exampleLocales.messages?.load_error
  })

  if (handled && result.data) {
    const data = result.data
    // Update VeeValidate form state with server data
    setValues({
      name: data.name,
      description: data.description || '',
      price: data.price,
      image_url: data.image_url || null,
      status: data.status,
      hex_color: data.hex_color || '#3B82F6',
    })
  } else if (!handled) {
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
    ? await exampleStore.updateExample(ExampleId.value, { ...formValues, image_url: values.image_url || undefined }, imageFile.value || undefined)
    : await exampleStore.createExample(formValues, imageFile.value || undefined);

  const handled = handleApiResult(result, {
    setErrors,
    fieldNames: Object.keys(values),
    successTitle: exampleLocales.common?.success,
    successMessage: isEdit.value
        ? (exampleLocales.messages?.update_success || 'Updated')
        : (exampleLocales.messages?.create_success || 'Created'),
    errorTitle: exampleLocales.common?.error,
    genericError: exampleLocales.messages?.load_error
  });

  if (handled) {
    router.push({ name: 'testing.examples.list' });
  }
});

/** Watch for ID changes (e.g., when clicking a similar item) to reload the form */
watch(() => route.params.id, () => {
  loadExample()
})

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
            <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
              {{ isEdit ? exampleLocales.titles.edit : exampleLocales.titles.create }}
            </h2>
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
                  class="w-full rounded-xl"
                  :invalid="!!errors.name"
                  :placeholder="exampleLocales.placeholders?.name"
                />
                <small v-if="errors.name" class="font-medium text-danger">{{ errors.name }}</small>
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
                  class="w-full rounded-xl"
                  :invalid="!!errors.description"
                  :placeholder="exampleLocales.placeholders?.description"
                />
                <small v-if="errors.description" class="font-medium text-danger">{{
                  errors.description
                }}</small>
              </div>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <div class="flex flex-col gap-2">
                  <label class="font-bold text-surface-700 dark:text-surface-200">{{
                    exampleLocales.labels?.brand_color
                  }}</label>
                  <div class="flex items-stretch gap-3">
                    <ColorPicker
                      v-model="hex_color"
                      format="hex"
                      :invalid="!!errors.hex_color"
                      :pt="{
                        root: { class: 'flex w-8' },
                        input: { class: 'rounded-xl h-full w-full' }
                      }"
                    />
                    <InputText
                      v-model="hex_color"
                      class="flex-1 rounded-xl"
                      placeholder="#3B82F6"
                      :invalid="!!errors.hex_color"
                    />
                  </div>
                  <small v-if="errors.hex_color" class="font-medium text-danger">{{
                    errors.hex_color
                  }}</small>
                </div>

                <div class="flex flex-col gap-2">
                  <label class="font-bold text-surface-700 dark:text-surface-200">{{
                    exampleLocales.labels?.status
                  }}</label>
                  <Select
                    v-model="status"
                    :options="statusOptions"
                    optionLabel="label"
                    optionValue="value"
                    :placeholder="exampleLocales.placeholders?.select_status"
                    class="w-full rounded-xl"
                    :invalid="!!errors.status"
                    :pt="{
                      label: { class: 'py-1.5 flex items-center' },
                      option: ({ context }) => ({
                        class: [
                          'flex items-center gap-2 px-4 py-3 transition-all duration-200 cursor-pointer',
                          {
                            // Dynamic Hover Effects
                            'hover:bg-warn-50 dark:hover:bg-warn-950/20 hover:text-warn-600':
                              context.option.value === ExampleStatus.Draft,
                            'hover:bg-success-50 dark:hover:bg-success-950/20 hover:text-success-600':
                              context.option.value === ExampleStatus.Active,
                            'hover:bg-surface-100 dark:hover:bg-surface-800 hover:text-surface-700':
                              context.option.value === ExampleStatus.Archived,

                            // Selected State
                            'bg-surface-50 dark:bg-surface-800/50 border-l-4 border-primary':
                              context.selected,
                          },
                        ],
                      }),
                    }"
                  >
                    <template #value="slotProps">
                      <div v-if="slotProps.value !== null" class="flex items-center gap-2 leading-none">
                        <i
                          :class="statusOptions.find((o) => o.value === slotProps.value)?.icon"
                          class="text-surface-500"
                        ></i>
                        <Badge
                          :value="statusOptions.find((o) => o.value === slotProps.value)?.label"
                          :severity="STATUS_COLORS[slotProps.value as ExampleStatus].severity"
                          class="px-2 py-0 text-[10px] font-bold uppercase rounded-md h-5 flex items-center"
                        />
                      </div>
                    </template>
                    <template #option="slotProps">
                      <i :class="slotProps.option.icon" class="text-surface-500"></i>
                      <span class="text-sm font-medium">{{ slotProps.option.label }}</span>
                    </template>
                  </Select>
                  <small v-if="errors.status" class="font-medium text-danger">{{
                    errors.status
                  }}</small>
                </div>
              </div>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
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
                    class="w-full rounded-xl"
                    inputClass="rounded-xl"
                    :invalid="!!errors.price"
                  />
                  <small v-if="errors.price" class="font-medium text-danger">{{
                    errors.price
                  }}</small>
                </div>

                <div class="flex flex-col gap-2">
                  <label class="font-bold text-surface-700 dark:text-surface-200">{{
                    exampleLocales.labels?.category
                  }}</label>
                  <InputText
                    disabled
                    :value="exampleLocales.labels?.category_default"
                    class="bg-surface-50 dark:bg-surface-800 opacity-60 rounded-xl"
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
                  :value="statusOptions.find((o) => o.value === status)?.label"
                  :severity="STATUS_COLORS[status as ExampleStatus].severity"
                ></Badge>
              </div>
              <div v-if="hex_color" class="flex items-center justify-between text-sm">
                <span class="font-medium text-surface-500">{{ exampleLocales.labels?.brand_color }}</span>
                <div class="flex items-center gap-2">
                  <span class="font-mono text-xs text-surface-400">{{ hex_color.startsWith('#') ? hex_color : '#' + hex_color }}</span>
                  <div
                    class="w-4 h-4 border rounded-full border-surface-200"
                    :style="{ backgroundColor: hex_color.startsWith('#') ? hex_color : '#' + hex_color }"
                  ></div>
                </div>
              </div>
              <Divider class="my-0" />
              <div class="flex items-center justify-between">
                <span class="font-medium text-surface-500">{{
                  exampleLocales.labels?.live_price
                }}</span>
                <span class="text-xl font-black text-primary">{{
                  formatCurrency(price || 0)
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
