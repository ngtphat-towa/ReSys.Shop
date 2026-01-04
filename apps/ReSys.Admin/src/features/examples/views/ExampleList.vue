<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useExampleStore } from '../example.store';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm'
import { storeToRefs } from 'pinia';

const exampleStore = useExampleStore();
const { examples, loading, totalRecords, pagination } = storeToRefs(exampleStore)
const router = useRouter()
const confirm = useConfirm()
const search = ref('')

const loadExamples = async () => {
  await exampleStore.fetchExamples({ search: search.value })
}

const onPage = (event: any) => {
  pagination.value.page = event.page + 1
  pagination.value.pageSize = event.rows
  loadExamples()
}

const onSearch = () => {
  pagination.value.page = 1
  loadExamples()
}

const editExample = (id: string) => {
  router.push(`/Examples/edit/${id}`)
}

const confirmDelete = (Example: any) => {
  confirm.require({
    message: `Are you sure you want to delete "${Example.name}"? This action cannot be undone.`,
    header: 'Dangerous Action',
    icon: 'pi pi-info-circle',
    rejectLabel: 'Cancel',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true,
    },
    acceptProps: {
      label: 'Delete',
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
          Example Catalog
        </h1>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400"
            >Manage your store inventory and Example details.</span
          >
          <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
        </div>
      </div>
      <div class="flex w-full gap-3 md:w-auto">
        <IconField iconPosition="left" class="w-full md:w-72">
          <InputIcon class="pi pi-search" />
          <InputText
            v-model="search"
            placeholder="Search by name..."
            class="w-full rounded-xl"
            @keyup.enter="onSearch"
          />
        </IconField>
        <Button
          label="New Example"
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
        :paginator="true"
        :rows="pagination.pageSize"
        class="overflow-hidden border rounded-lg shadow-sm border-surface-100 dark:border-surface-800"
      >
        <template #empty>
          <div
            class="flex flex-col items-center justify-center py-20 text-surface-400 dark:text-surface-500"
          >
            <i class="mb-4 text-6xl pi pi-box opacity-20"></i>
            <p class="text-xl font-medium">No Examples found</p>
          </div>
        </template>

        <Column field="image_url" header="Preview" class="w-24">
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

        <Column field="name" header="Name" sortable>
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

        <Column field="description" header="Details" class="max-w-xs">
          <template #body="slotProps">
            <p class="text-sm italic text-surface-500 dark:text-surface-400 line-clamp-1">
              {{ slotProps.data.description || 'No additional details' }}
            </p>
          </template>
        </Column>

        <Column field="price" header="Price" sortable>
          <template #body="slotProps">
            <div class="flex flex-col">
              <span class="font-black text-surface-900 dark:text-surface-0">
                {{
                  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                    slotProps.data.price,
                  )
                }}
              </span>
              <span class="text-[10px] text-emerald-500 font-bold uppercase">In Stock</span>
            </div>
          </template>
        </Column>

        <Column header="Status" class="w-24">
          <template #body>
            <div class="flex items-center gap-2">
              <div class="w-2 h-2 rounded-full bg-emerald-500"></div>
              <span class="text-xs font-medium text-surface-700 dark:text-surface-200">Active</span>
            </div>
          </template>
        </Column>

        <Column header="Actions" class="w-32 text-right">
          <template #body="slotProps">
            <div class="flex justify-end gap-1">
              <Button
                icon="pi pi-pencil"
                severity="secondary"
                text
                rounded
                @click="editExample(slotProps.data.id)"
                v-tooltip.top="'Edit item'"
              />
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                @click="confirmDelete(slotProps.data)"
                v-tooltip.top="'Delete item'"
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
