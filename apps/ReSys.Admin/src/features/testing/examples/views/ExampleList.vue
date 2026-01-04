<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useExampleStore } from '../example.store';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm'
import { storeToRefs } from 'pinia';
import { exampleLocales } from '../example.locales';
import { FilterMatchMode, FilterOperator } from '@primevue/core/api';
import { showToast } from '@/shared/api/client';

const exampleStore = useExampleStore();
const { examples, loading, totalRecords, query } = storeToRefs(exampleStore)
const router = useRouter()
const confirm = useConfirm()

// 1. Correct Initialization for 'menu' filter mode
const filters = ref<any>({
    global: { value: query.value.search, matchMode: FilterMatchMode.CONTAINS },
    name: {
        operator: FilterOperator.AND,
        constraints: [{ value: query.value.name || null, matchMode: FilterMatchMode.CONTAINS }]
    },
    price: {
        operator: FilterOperator.AND,
        constraints: [{ value: query.value.min_price || null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO }]
    },
    status: {
        operator: FilterOperator.OR,
        constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }]
    }
});

const statuses = ref(['Active', 'Inactive', 'Out of Stock']);

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
        page: 1
    });
};

const onFilter = () => {
    exampleStore.fetchExamples({
        search: filters.value.global.value,
        name: filters.value.name.constraints[0]?.value,
        min_price: filters.value.price.constraints[0]?.value,
        page: 1
    });
};

const clearFilters = () => {
    filters.value = {
        global: { value: null, matchMode: FilterMatchMode.CONTAINS },
        name: {
            operator: FilterOperator.AND,
            constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }]
        },
        price: {
            operator: FilterOperator.AND,
            constraints: [{ value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO }]
        },
        status: {
            operator: FilterOperator.OR,
            constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }]
        }
    };
    onFilter();
};

const editExample = (id: string) => {
  router.push(`/Examples/edit/${id}`)
}

const confirmDelete = (Example: any) => {
  confirm.require({
    message: (exampleLocales.confirm!.delete_message as Function)(Example.name),
    header: exampleLocales.confirm!.delete_header as string,
    icon: 'pi pi-info-circle',
    rejectLabel: exampleLocales.confirm!.reject_label as string,
    rejectProps: {
      label: exampleLocales.confirm!.reject_label as string,
      severity: 'secondary',
      outlined: true,
    },
    acceptProps: {
      label: exampleLocales.confirm!.accept_label as string,
      severity: 'danger',
    },
    accept: async () => {
      const result = await exampleStore.deleteExample(Example.id);
      if (result.success) {
          showToast('success', 'Deleted', 'Example has been removed.');
      }
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
          {{ exampleLocales.titles.list }}
        </h1>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ exampleLocales.descriptions?.list }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
        </div>
      </div>
      <div class="flex w-full gap-3 md:w-auto">
        <Button
          :label="exampleLocales.actions.new"
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
        v-model:filters="filters"
        :value="examples"
        :loading="loading"
        :totalRecords="totalRecords"
        :lazy="true"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        :paginator="true"
        :rows="query.page_size"
        :first="(query?.page - 1) * query?.page_size"
        :sortField="query.sort_by"
        :sortOrder="query.is_descending ? -1 : 1"
        filterDisplay="menu"
        removableSort
        class="overflow-hidden border rounded-lg shadow-sm border-surface-100 dark:border-surface-800"
      >
        <template #header>
            <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
                <IconField iconPosition="left" class="w-full md:w-72">
                    <InputIcon class="pi pi-search" />
                    <InputText v-model="filters.global.value" :placeholder="exampleLocales.placeholders?.search" @keyup.enter="onFilter" class="w-full rounded-xl" />
                </IconField>

                <Button
                    type="button"
                    icon="pi pi-filter-slash"
                    :label="exampleLocales.table?.clear_filter"
                    outlined
                    @click="clearFilters"
                    class="w-full rounded-xl md:w-auto"
                />
            </div>
        </template>

        <template #empty>
          <div
            class="flex flex-col items-center justify-center py-20 text-surface-400 dark:text-surface-500"
          >
            <i class="mb-4 text-6xl pi pi-box opacity-20"></i>
            <p class="text-xl font-medium">{{ exampleLocales.messages?.empty_list }}</p>
          </div>
        </template>

        <Column field="image_url" :header="exampleLocales.table?.preview" class="w-24">
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

        <Column field="name" :header="exampleLocales.table?.name" sortable>
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
          <template #filter="{ filterModel, filterCallback }">
                <InputText v-model="filterModel.value" type="text" @keyup.enter="filterCallback()" class="p-column-filter" :placeholder="exampleLocales.table?.filter_placeholder" />
            </template>
        </Column>

        <Column field="description" :header="exampleLocales.table?.details" class="max-w-xs">
          <template #body="slotProps">
            <p class="text-sm italic text-surface-500 dark:text-surface-400 line-clamp-1">
              {{ slotProps.data.description || exampleLocales.table?.no_details }}
            </p>
          </template>
        </Column>

        <Column field="price" :header="exampleLocales.table?.price" sortable dataType="numeric">
          <template #body="slotProps">
            <div class="flex flex-col">
              <span class="font-black text-surface-900 dark:text-surface-0">
                {{
                  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(
                    slotProps.data.price,
                  )
                }}
              </span>
              <span class="text-[10px] text-emerald-500 font-bold uppercase">{{ exampleLocales.table?.in_stock }}</span>
            </div>
          </template>
          <template #filter="{ filterModel, filterCallback }">
                <InputNumber v-model="filterModel.value" mode="currency" currency="USD" locale="en-US" @keyup.enter="filterCallback()" class="w-full" />
            </template>
        </Column>

        <Column field="status" :header="exampleLocales.table?.status" class="w-24">
          <template #body>
            <div class="flex items-center gap-2">
              <div class="w-2 h-2 rounded-full bg-emerald-500"></div>
              <span class="text-xs font-medium text-surface-700 dark:text-surface-200">{{ exampleLocales.table?.active }}</span>
            </div>
          </template>
          <template #filter="{ filterModel, filterCallback }">
                <Select v-model="filterModel.value" :options="statuses" placeholder="Select Status" @change="filterCallback()" class="p-column-filter" style="min-width: 12rem" :showClear="true">
                    <template #option="slotProps">
                        <Badge :value="slotProps.option" :severity="slotProps.option === 'Active' ? 'success' : 'secondary'"></Badge>
                    </template>
                </Select>
            </template>
        </Column>

        <Column :header="exampleLocales.table?.actions" class="w-32 text-right">
          <template #body="slotProps">
            <div class="flex justify-end gap-1">
              <Button
                icon="pi pi-pencil"
                severity="secondary"
                text
                rounded
                @click="editExample(slotProps.data.id)"
                v-tooltip.top="exampleLocales.tooltips?.edit"
              />
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                @click="confirmDelete(slotProps.data)"
                v-tooltip.top="exampleLocales.tooltips?.delete"
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
  padding: 1rem;
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
