<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useExampleCategoryStore } from '../example-category.store'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { storeToRefs } from 'pinia'
import { exampleCategoryLocales } from '../example-category.locales'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable'
import { useToast } from '@/shared/composables/toast.use'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'
import { QueryBuilder } from '@/shared/utils/query-builder.utils'
import type { ExampleCategoryListItem } from '../example-category.types'

const categoryStore = useExampleCategoryStore()
const { categories, loading, totalRecords, query } = storeToRefs(categoryStore)
const router = useRouter()
const confirm = useConfirm()
const { showToast } = useToast()

const stringFilterOptions = ref([
  { label: exampleCategoryLocales.filters?.contains || 'Contains', value: FilterMatchMode.CONTAINS },
  { label: exampleCategoryLocales.filters?.equals || 'Equals', value: FilterMatchMode.EQUALS },
])

const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
  name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
  },
})

const loadCategories = async () => {
  await categoryStore.fetchCategories()
}

const onPage = (event: DataTablePageEvent) => {
  categoryStore.fetchCategories({
    page: event.page !== undefined ? event.page + 1 : 1,
    page_size: event.rows,
  })
}

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder()
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc')
  }

  categoryStore.fetchCategories({
    sort: builder.build().sort,
    page: 1,
  })
}

const onFilter = () => {
  const globalFilter = filters.value.global as { value: string | null }
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] }

  const builder = new QueryBuilder()
  if (nameFilter.constraints[0]?.value) {
    builder.where('Name', '*', nameFilter.constraints[0].value)
  }

  const built = builder.build()

  categoryStore.fetchCategories({
    search: globalFilter.value || undefined,
    search_field: globalFilter.value ? ['Name', 'Description'] : undefined,
    filter: built.filter,
    page: 1,
  })
}

const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
    },
  }
  onFilter()
}

const editCategory = (id: string) => {
  router.push({ name: 'testing.example-categories.edit', params: { id } })
}

const confirmDelete = (category: ExampleCategoryListItem) => {
  confirm.require({
    message: (exampleCategoryLocales.confirm!.delete_message as (name: string) => string)(
      category.name,
    ),
    header: exampleCategoryLocales.confirm!.delete_header as string,
    icon: 'pi pi-info-circle',
    rejectLabel: exampleCategoryLocales.confirm!.reject_label as string,
    acceptLabel: exampleCategoryLocales.confirm!.accept_label as string,
    acceptProps: {
      severity: 'danger',
    },
    accept: async () => {
      const result = await categoryStore.deleteCategory(category.id)
      if (result.success) {
        showToast(
          'success',
          exampleCategoryLocales.common?.success || 'Deleted',
          exampleCategoryLocales.messages?.delete_success || 'Category has been removed.',
        )
      }
    },
  })
}

onMounted(() => {
  loadCategories()
})
</script>

<template>
  <div class="p-6">
    <AppBreadcrumb :locales="exampleCategoryLocales" />
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
          {{ exampleCategoryLocales.titles.list }}
        </h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ exampleCategoryLocales.descriptions?.list }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
        </div>
      </div>
      <div class="flex w-full gap-3 md:w-auto">
        <Button
          :label="exampleCategoryLocales.actions.new"
          icon="pi pi-plus"
          @click="router.push({ name: 'testing.example-categories.create' })"
          class="px-4 shadow-lg rounded-xl"
        />
      </div>
    </div>

    <div
      class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800"
    >
      <DataTable
        v-model:filters="filters"
        :value="categories"
        :loading="loading"
        :totalRecords="totalRecords"
        :lazy="true"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        :paginator="true"
        :rows="query.page_size || 10"
        :first="((query.page || 1) - 1) * (query.page_size || 10)"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
        class="overflow-hidden"
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText
                v-model="(filters.global as any).value"
                :placeholder="exampleCategoryLocales.placeholders?.search"
                @keyup.enter="onFilter"
                class="w-full rounded-xl"
              />
            </IconField>

            <Button
              type="button"
              icon="pi pi-filter-slash"
              :label="exampleCategoryLocales.table?.clear_filter"
              outlined
              @click="clearFilters"
              class="w-full rounded-xl md:w-auto"
            />
          </div>
        </template>

        <template #empty>
          <div
            class="flex flex-col items-center justify-center py-20 text-surface-400 dark:text-surface-50"
          >
            <i class="mb-4 text-6xl pi pi-box opacity-20"></i>
            <p class="text-xl font-medium">{{ exampleCategoryLocales.messages?.empty_list }}</p>
          </div>
        </template>

        <Column
          field="name"
          :header="exampleCategoryLocales.table?.name"
          sortable
          :filterMatchModeOptions="stringFilterOptions"
          :showFilterOperator="false"
          :showAddButton="false"
        >
          <template #body="slotProps">
            <span class="font-bold text-surface-900 dark:text-surface-0">{{
              slotProps.data.name
            }}</span>
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @keyup.enter="filterCallback()"
              class="p-column-filter"
              :placeholder="exampleCategoryLocales.table?.filter_placeholder"
            />
          </template>
        </Column>

        <Column field="description" :header="exampleCategoryLocales.table?.details">
          <template #body="slotProps">
            <p class="text-sm italic text-surface-500 dark:text-surface-400 line-clamp-1">
              {{ slotProps.data.description || exampleCategoryLocales.table?.no_details }}
            </p>
          </template>
        </Column>

        <Column
          :header="exampleCategoryLocales.table?.actions"
          class="w-32 text-right"
          frozen
          alignFrozen="right"
        >
          <template #body="slotProps">
            <div class="flex justify-end gap-1">
              <Button
                icon="pi pi-pencil"
                severity="secondary"
                text
                rounded
                @click="editCategory(slotProps.data.id)"
                v-tooltip.top="exampleCategoryLocales.tooltips?.edit"
              />
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                @click="confirmDelete(slotProps.data)"
                v-tooltip.top="exampleCategoryLocales.tooltips?.delete"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
/**
 * VUE COMPONENT STYLING GUIDE (Scoped & Deep)
 *
 * 1. THE 'scoped' ATTRIBUTE:
 *    - PURPOSE: Enforces CSS encapsulation. Any styles defined here are strictly local to this component.
 *    - MECHANISM: The Vue compiler generates a unique data attribute (e.g., [data-v-f3f3eg]) and attaches
 *      it to every HTML element in this template. It then rewrites your CSS selectors to include
 *      this attribute, preventing "CSS Leakage" to other parts of the application.
 *    - USAGE: Used by default in all feature views to ensure UI stability and prevent accidental regressions.
 *
 * 2. THE ':deep()' PSEUDO-CLASS (Deep Combinator):
 *    - PURPOSE: Allows targeting elements inside child components or third-party libraries (like PrimeVue).
 *    - WHY IT'S NEEDED: Because of 'scoped' encapsulation, your styles cannot "see" inside components
 *      like <DataTable>. The tags inside the table (like <thead> or <td>) are managed by the library
 *      and do not receive this component's unique data attribute.
 *    - MECHANISM: Tells the CSS post-processor to match the child element regardless of the scope attribute.
 *    - USAGE: Crucial for customizing the "look and feel" of third-party UI frameworks without
 *      polluting the global CSS namespace.
 */

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
</style>
