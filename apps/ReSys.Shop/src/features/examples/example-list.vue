<script setup lang="ts">
/**
 * Shop Example List View
 * Provides a consumer-facing catalog for browsing and searching items.
 * Demonstrates a card-based layout with responsive design.
 */
import { onMounted, ref } from 'vue'
import { useExampleStore } from './example.store'
import { storeToRefs } from 'pinia'
import { exampleLocales } from './example.locales'

// --- STORE & SEARCH STATE ---
const exampleStore = useExampleStore()
const { examples, loading } = storeToRefs(exampleStore)
const search = ref('')

/**
 * Loads the catalog based on search input.
 * Default page size is set to 20 for optimal browsing experience.
 */
const loadExamples = async () => {
  await exampleStore.fetchExamples({
    search: search.value,
    page_size: 20,
  })
}

/**
 * Triggered when user submits search.
 */
const onSearch = () => {
  loadExamples()
}

/**
 * Clears the search input and refreshes the full catalog.
 */
const onReset = () => {
  search.value = ''
  onSearch()
}

onMounted(() => {
  loadExamples()
})
</script>

<template>
  <div class="container p-4 mx-auto md:p-8">
    <!-- Hero Section / Search -->
    <div
      class="mb-12 flex flex-col md:flex-row justify-between items-center bg-surface-0 dark:bg-surface-900 p-8 md:p-12 rounded-[2.5rem] shadow-sm border border-surface-100 dark:border-surface-800 gap-8 transition-colors duration-300"
    >
      <div class="text-center md:text-left">
        <h1
          class="mb-4 text-5xl font-black leading-none tracking-tight text-surface-900 dark:text-surface-0"
        >
          {{ exampleLocales.titles.hero_primary }} <span class="text-primary">{{ exampleLocales.titles.hero_highlight }}</span>
        </h1>
        <p class="max-w-md text-lg text-surface-500 dark:text-surface-400">
          {{ exampleLocales.descriptions?.hero }}
        </p>
      </div>
      <div class="flex w-full max-w-lg gap-3">
        <IconField iconPosition="left" class="flex-1 overflow-hidden shadow-sm rounded-2xl">
          <InputIcon class="pi pi-search" />
          <InputText
            v-model="search"
            :placeholder="exampleLocales.placeholders?.search"
            class="w-full py-4 text-lg transition-colors border-none bg-surface-50 dark:bg-surface-800 focus:bg-surface-0 dark:focus:bg-surface-950"
            @keyup.enter="onSearch"
          />
        </IconField>
        <Button icon="pi pi-search" class="p-4 px-6 shadow-lg rounded-2xl" @click="onSearch" />
      </div>
    </div>

    <!-- Loading Skeleton Grid -->
    <div v-if="loading" class="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-4">
      <div
        v-for="i in 8"
        :key="i"
        class="bg-surface-0 dark:bg-surface-900 rounded-[2rem] overflow-hidden shadow-sm border border-surface-100 dark:border-surface-800 p-4"
      >
        <Skeleton width="100%" height="250px" borderRadius="1.5rem" class="mb-4"></Skeleton>
        <Skeleton width="60%" height="1.5rem" class="mb-2"></Skeleton>
        <Skeleton width="40%" height="2rem" class="mb-4"></Skeleton>
        <Skeleton width="100%" height="3rem" borderRadius="1rem"></Skeleton>
      </div>
    </div>

    <!-- Empty State / No Results -->
    <div
      v-else-if="examples.length === 0"
      class="text-center py-32 bg-surface-50 dark:bg-surface-800/50 rounded-[3rem] border-2 border-dashed border-surface-200 dark:border-surface-700"
    >
      <div
        class="flex items-center justify-center w-24 h-24 mx-auto mb-6 rounded-full shadow-md bg-surface-0 dark:bg-surface-900"
      >
        <i class="text-4xl pi pi-search text-surface-300 dark:text-surface-600"></i>
      </div>
      <h2 class="mb-2 text-3xl font-black text-surface-800 dark:text-surface-100">
        {{ exampleLocales.messages?.empty_list }}
      </h2>
      <p class="max-w-sm mx-auto text-lg text-surface-400 dark:text-surface-500">
        {{ exampleLocales.messages?.empty_description }}
      </p>
      <Button
        :label="exampleLocales.actions.view_all"
        class="mt-8 font-bold p-button-text"
        @click="onReset"
      />
    </div>

    <!-- Catalog Grid -->
    <div v-else class="grid grid-cols-1 gap-10 md:grid-cols-2 lg:grid-cols-4">
      <Card
        v-for="example in examples"
        :key="example.id"
        class="overflow-hidden border-none shadow-sm hover:shadow-2xl transition-all duration-500 transform hover:-translate-y-2 rounded-[2.5rem] group bg-surface-0 dark:bg-surface-900 border border-surface-50 dark:border-surface-800"
      >
        <template #header>
          <div
            class="relative overflow-hidden aspect-[4/5] m-4 rounded-[2rem] shadow-inner bg-surface-50 dark:bg-surface-800"
          >
            <img
              v-if="example.image_url"
              :src="example.image_url"
              :alt="example.name"
              class="object-cover w-full h-full transition-transform duration-700 group-hover:scale-110"
            />
            <div
              v-else
              class="flex items-center justify-center w-full h-full transition-colors text-surface-200 dark:text-surface-700"
            >
              <i class="text-6xl pi pi-image"></i>
            </div>
            <div class="absolute top-4 right-4">
              <Button
                icon="pi pi-heart"
                severity="secondary"
                rounded
                text
                class="w-12 h-12 shadow-sm bg-surface-0/90 dark:bg-surface-900/90 backdrop-blur-md hover:text-red-500"
              />
            </div>
          </div>
        </template>
        <template #title>
          <h3
            class="mb-1 text-xl font-black tracking-tight text-surface-900 dark:text-surface-0 line-clamp-1"
          >
            {{ example.name }}
          </h3>
        </template>
        <template #subtitle>
          <span class="text-3xl font-black text-primary">
            {{
              new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                example.price,
              )
            }}
          </span>
        </template>
        <template #content>
          <p
            class="mt-2 text-sm italic leading-relaxed text-surface-500 dark:text-surface-400 line-clamp-2"
          >
            {{ example.description || exampleLocales.table?.no_details }}
          </p>
        </template>
        <template #footer>
          <div class="flex gap-3 mt-4">
            <Button
              :label="exampleLocales.actions.details"
              severity="secondary"
              outlined
              class="flex-1 py-3 font-bold rounded-2xl border-surface-200 dark:border-surface-700"
              @click="$router.push({ name: 'shop.examples.detail', params: { id: example.id } })"
            />
            <Button
              :label="exampleLocales.actions.add"
              icon="pi pi-shopping-bag"
              class="flex-1 py-3 font-bold shadow-lg rounded-2xl shadow-primary/20"
            />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-card-body) {
  padding: 0 2rem 2rem 2rem;
}
:deep(.p-card-title) {
  margin: 0;
}
:deep(.p-card-subtitle) {
  margin: 0;
}
:deep(.p-card-footer) {
  padding-top: 0;
}
</style>
