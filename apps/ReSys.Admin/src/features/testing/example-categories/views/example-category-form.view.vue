<script setup lang="ts">
import { onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useExampleCategoryStore } from '../example-category.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { ExampleCategorySchema } from '../example-category.schema'
import { exampleCategoryLocales } from '../example-category.locales'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'

const route = useRoute()
const router = useRouter()
const categoryStore = useExampleCategoryStore()
const { loading } = storeToRefs(categoryStore)
const { handleApiResult } = useApiErrorHandler()

const isEdit = computed(() => route.params.id !== undefined)
const categoryId = computed(() => route.params.id as string)

const { defineField, handleSubmit, errors, setValues, setErrors, values, resetForm } = useForm({
  validationSchema: toTypedSchema(ExampleCategorySchema),
  initialValues: {
    name: '',
    description: '',
  },
})

const [name] = defineField('name')
const [description] = defineField('description')

const loadCategory = async () => {
  if (!isEdit.value) {
    categoryStore.clearCurrent()
    resetForm()
    return
  }

  const result = await categoryStore.fetchCategoryById(categoryId.value)
  const handled = handleApiResult(result, {
    errorTitle: exampleCategoryLocales.common?.error,
    genericError: exampleCategoryLocales.messages?.load_error
  })

  if (handled && result.data) {
    setValues({
      name: result.data.name,
      description: result.data.description || '',
    })
  } else if (!handled) {
    router.push({ name: 'testing.example-categories.list' })
  }
}

const onSubmit = handleSubmit(async (formValues) => {
  const result = isEdit.value
    ? await categoryStore.updateCategory(categoryId.value, formValues)
    : await categoryStore.createCategory(formValues)

  const handled = handleApiResult(result, {
    setErrors,
    fieldNames: Object.keys(values),
    successTitle: exampleCategoryLocales.common?.success,
    successMessage: isEdit.value
        ? (exampleCategoryLocales.messages?.update_success || 'Updated')
        : (exampleCategoryLocales.messages?.create_success || 'Created'),
    errorTitle: exampleCategoryLocales.common?.error,
  });

  if (handled) {
    router.push({ name: 'testing.example-categories.list' });
  }
});

watch(() => route.params.id, () => {
  loadCategory()
})

onMounted(() => {
  loadCategory()
})
</script>

<template>
  <form @submit="onSubmit" class="max-w-4xl p-6 mx-auto">
    <div class="mb-6">
      <AppBreadcrumb :locales="exampleCategoryLocales" />

      <div class="flex flex-col justify-between gap-4 md:flex-row md:items-center">
        <div class="flex items-center gap-4">
          <Button
            type="button"
            icon="pi pi-arrow-left"
            text
            rounded
            severity="secondary"
            @click="router.push({ name: 'testing.example-categories.list' })"
            class="bg-surface-100 dark:bg-surface-800"
          />
          <div>
            <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
              {{ isEdit ? exampleCategoryLocales.titles.edit : exampleCategoryLocales.titles.create }}
            </h2>
            <p class="text-sm text-surface-500 dark:text-surface-400">
              {{
                isEdit
                  ? `${exampleCategoryLocales.messages?.modifying || 'Modifying'}: ${name || exampleCategoryLocales.messages?.loading}`
                  : (exampleCategoryLocales.descriptions?.create || 'Fill in the details to add a new category.')
              }}
            </p>
          </div>
        </div>
        <div class="flex gap-2">
          <Button
            type="button"
            :label="exampleCategoryLocales.actions.cancel"
            severity="secondary"
            text
            @click="router.push({ name: 'testing.example-categories.list' })"
          />
          <Button
            type="submit"
            :label="isEdit ? exampleCategoryLocales.actions.save_edit : exampleCategoryLocales.actions.save_create"
            icon="pi pi-check"
            :loading="loading"
            class="px-6 shadow-lg rounded-xl"
          />
        </div>
      </div>
    </div>

    <Card class="border-none shadow-sm rounded-2xl bg-surface-0 dark:bg-surface-900">
      <template #title>
        <span class="text-xl font-bold text-surface-800 dark:text-surface-50">{{
          exampleCategoryLocales.titles.basic_info
        }}</span>
      </template>
      <template #content>
        <div class="flex flex-col gap-6 pt-2">
          <div class="flex flex-col gap-2">
            <label for="name" class="font-bold text-surface-700 dark:text-surface-200">{{
              exampleCategoryLocales.labels?.name
            }}</label>
            <InputText
              id="name"
              v-model="name"
              class="w-full rounded-xl"
              :invalid="!!errors.name"
              :placeholder="exampleCategoryLocales.placeholders?.name"
            />
            <small v-if="errors.name" class="font-medium text-danger">{{ errors.name }}</small>
          </div>

          <div class="flex flex-col gap-2">
            <label for="description" class="font-bold text-surface-700 dark:text-surface-200">{{
              exampleCategoryLocales.labels?.description
            }}</label>
            <Textarea
              id="description"
              v-model="description"
              rows="6"
              class="w-full rounded-xl"
              :invalid="!!errors.description"
              :placeholder="exampleCategoryLocales.placeholders?.description"
            />
            <small v-if="errors.description" class="font-medium text-danger">{{
              errors.description
            }}</small>
          </div>
        </div>
      </template>
    </Card>
  </form>
</template>
