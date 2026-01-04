<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useExampleStore } from '../example.store';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm'
import { storeToRefs } from 'pinia';
import { ExampleLocales } from '../example.locales';

const exampleStore = useExampleStore();
const { examples, loading, totalRecords, query } = storeToRefs(exampleStore)
const router = useRouter()
const confirm = useConfirm()

// Temporary local search state to avoid triggering API on every keystroke
const searchInput = ref(query.value.search || '');

const loadExamples = async () => {
  await exampleStore.fetchExamples()
}

const onPage = (event: any) => {
  exampleStore.fetchExamples({
      page: event.page + 1,
      page_size: event.rows
  });
}

const onSort = (event: any) => {
    exampleStore.fetchExamples({
        sort_by: event.sortField,
        is_descending: event.sortOrder === -1,
        page: 1 // Reset to first page on sort
    });
};

const onSearch = () => {
  exampleStore.fetchExamples({
      search: searchInput.value,
      page: 1 // Reset to first page on search
  });
}

const editExample = (id: string) => {
  router.push(`/Examples/edit/${id}`)
}

const confirmDelete = (Example: any) => {
  confirm.require({
    message: ExampleLocales.confirm.delete_message(Example.name),
    header: ExampleLocales.confirm.delete_header,
    icon: 'pi pi-info-circle',
    rejectLabel: ExampleLocales.confirm.reject_label,
    rejectProps: {
      label: ExampleLocales.confirm.reject_label,
      severity: 'secondary',
      outlined: true,
    },
    acceptProps: {
      label: ExampleLocales.confirm.accept_label,
      severity: 'danger',
    },
    accept: async () => {
      await exampleStore.deleteExample(Example.id)
    },
  })
}

onMounted(() => {
  loadExamples()
})
</script>

<template>
  <div class="p-6">
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h1 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-0">
          {{ ExampleLocales.titles.list }}
        </h1>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ ExampleLocales.descriptions.list }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
        </div>
      </div>
      <div class="flex w-full gap-3 md:w-auto">
        <IconField iconPosition="left" class="w-full md:w-72">
          <InputIcon class="pi pi-search" />
          <InputText
            v-model="searchInput"
            :placeholder="ExampleLocales.placeholders.search"
            class="w-full rounded-xl"
            @keyup.enter="onSearch"
          />
        </IconField>
        <Button
          :label="ExampleLocales.actions.new"
          icon="pi pi-plus"
          @click="router.push('/Examples/create')"
          class="px-4 shadow-lg rounded-xl"
        />
      </div>
    </div>

    <div
      class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800"
    >
      <DataTable
        :value="examples"
        :loading="loading"
        :totalRecords="totalRecords"
        :lazy="true"
        @page="onPage"
        @sort="onSort"
        :paginator="true"
        :rows="query.page_size"
        :first="(query.page! - 1) * query.page_size!"
        :sortField="query.sort_by"
        :sortOrder="query.is_descending ? -1 : 1"
        removableSort
        class="overflow-hidden border rounded-lg shadow-sm border-surface-100 dark:border-surface-800"
      >
        <template #empty>
          <div
            class="flex flex-col items-center justify-center py-20 text-surface-400 dark:text-surface-500"
          >
            <i class="mb-4 text-6xl pi pi-box opacity-20"></i>
            <p class="text-xl font-medium">{{ ExampleLocales.messages.empty_list }}</p>
          </div>
        </template>

        <Column field="image_url" :header="ExampleLocales.table.preview" class="w-24">
          <template #body="slotProps">
            <div
              class="relative overflow-hidden border shadow-sm w-14 h-14 rounded-xl bg-surface-50 dark:bg-surface-800 border-surface-100 dark:border-surface-700 group"
            >
              <img
                v-if="slotProps.data.image_url"
                :src="slotProps.data.image_url"
                :alt="slotProps.data.name"
                class="object-cover w-full h-full transition-transform group-hover:scale-110"
              />
              <div
                v-else
                class="flex items-center justify-center w-full h-full text-surface-300 dark:text-surface-600"
              >
                <i class="text-xl pi pi-image"></i>
              </div>
            </div>
          </template>
        </Column>

        <Column field="name" :header="ExampleLocales.table.name" sortable>
          <template #body="slotProps">
            <div class="flex flex-col">
              <span class="font-bold text-surface-900 dark:text-surface-0">{{
                slotProps.data.name
              }}</span>
              <span class="text-xs truncate text-surface-500 max-w-50"
                >ID: {{ slotProps.data.id }}</span
              >
            </div>
          </template>
        </Column>

        <Column field="description" :header="ExampleLocales.table.details" class="max-w-xs">
          <template #body="slotProps">
            <p class="text-sm italic text-surface-500 dark:text-surface-400 line-clamp-1">
              {{ slotProps.data.description || ExampleLocales.table.no_details }}
            </p>
          </template>
        </Column>

        <Column field="price" :header="ExampleLocales.table.price" sortable>
          <template #body="slotProps">
            <div class="flex flex-col">
              <span class="font-black text-surface-900 dark:text-surface-0">
                {{
                  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                    slotProps.data.price,
                  )
                }}
              </span>
              <span class="text-[10px] text-emerald-500 font-bold uppercase">{{ ExampleLocales.table.in_stock }}</span>
            </div>
          </template>
        </Column>

        <Column :header="ExampleLocales.table.status" class="w-24">
          <template #body>
            <div class="flex items-center gap-2">
              <div class="w-2 h-2 rounded-full bg-emerald-500"></div>
              <span class="text-xs font-medium text-surface-700 dark:text-surface-200">{{ ExampleLocales.table.active }}</span>
            </div>
          </template>
        </Column>

        <Column :header="ExampleLocales.table.actions" class="w-32 text-right">
          <template #body="slotProps">
            <div class="flex justify-end gap-1">
              <Button
                icon="pi pi-pencil"
                severity="secondary"
                text
                rounded
                @click="editExample(slotProps.data.id)"
                v-tooltip.top="ExampleLocales.tooltips.edit"
              />
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                @click="confirmDelete(slotProps.data)"
                v-tooltip.top="ExampleLocales.tooltips.delete"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-datatable-header) {
  background: transparent;
  padding: 0;
}
:deep(.p-datatable-thead > tr > th) {
  background: var(--p-content-background);
  color: var(--p-text-color);
  font-size: 0.875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  padding: 1rem 1.5rem;
  border-bottom: 2px solid var(--p-primary-color);
}
:deep(.p-datatable-tbody > tr > td) {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--p-content-border-color);
}
:deep(.p-datatable-tbody > tr:hover) {
  background: var(--p-primary-50);
}
.dark :deep(.p-datatable-tbody > tr:hover) {
  background: var(--p-primary-900);
}
</style>
