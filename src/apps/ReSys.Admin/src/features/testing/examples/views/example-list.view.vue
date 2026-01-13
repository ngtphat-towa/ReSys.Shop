<script setup lang="ts">
/**
 * Example List View
 * Demonstrates a "Lazy" PrimeVue DataTable with server-side pagination, sorting, and filtering.
 * Orchestrates communication between the ExampleStore and the DataTable UI.
 */
import { onMounted, ref } from 'vue'
import { useExampleStore } from '../example.store'
import { useExampleCategoryStore } from '../../example-categories/example-category.store'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { storeToRefs } from 'pinia'
import { ExampleStatus, STATUS_COLORS, type ExampleListItem } from '../example.types'
import { exampleLocales } from '../example.locales'
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api'
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable'
import { useToast } from '@/shared/composables/toast.use'
import { useFormatter } from '@/shared/composables/formatter.use'
import { QueryBuilder, type FilterOperator } from '@/shared/utils/query-builder.utils'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'

// --- STORE & ROUTING ---
const exampleStore = useExampleStore()
const categoryStore = useExampleCategoryStore()
const { examples, loading, totalRecords, query } = storeToRefs(exampleStore)
const { categories } = storeToRefs(categoryStore)
const router = useRouter()
const confirm = useConfirm()
const { formatCurrency } = useFormatter()
const { showToast } = useToast()

/**
 * Custom filter operator sets to keep the UI clean.
 */
const stringFilterOptions = ref([
  { label: exampleLocales.filters?.contains || 'Contains', value: FilterMatchMode.CONTAINS },
  { label: exampleLocales.filters?.equals || 'Equals', value: FilterMatchMode.EQUALS },
  { label: exampleLocales.filters?.starts_with || 'Starts With', value: FilterMatchMode.STARTS_WITH },
])

const numericFilterOptions = ref([
  { label: exampleLocales.filters?.equals || 'Equals', value: FilterMatchMode.EQUALS },
  { label: exampleLocales.filters?.not_equals || 'Not Equals', value: FilterMatchMode.NOT_EQUALS },
  { label: exampleLocales.filters?.greater_than || 'Greater Than', value: FilterMatchMode.GREATER_THAN },
  {
    label: exampleLocales.filters?.greater_than_equal || '>= Min',
    value: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO,
  },
  { label: exampleLocales.filters?.less_than || 'Less Than', value: FilterMatchMode.LESS_THAN },
  { label: exampleLocales.filters?.less_than_equal || '<= Max', value: FilterMatchMode.LESS_THAN_OR_EQUAL_TO },
])

/**
 * PrimeVue Filter Configuration
 * Maps internal UI filters to the API query parameters.
 */
const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
  name: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: query.value.name || null, matchMode: FilterMatchMode.CONTAINS }],
  },
  price: {
    operator: PrimeFilterOperator.AND,
    constraints: [
      { value: query.value.min_price || null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
    ],
  },
  status: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: query.value.status || null, matchMode: FilterMatchMode.IN }],
  },
  category_name: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
})

const statuses = ref([
  { label: 'Draft', value: ExampleStatus.Draft },
  { label: 'Active', value: ExampleStatus.Active },
  { label: 'Archived', value: ExampleStatus.Archived },
])

// --- DATA ACTIONS ---

/**
 * Initial data fetch.
 * This is called when the component is mounted to populate the initial list.
 */
const loadExamples = async () => {
  await Promise.all([
    exampleStore.fetchExamples(),
    categoryStore.fetchCategories({ page_size: 100 }),
  ])
}

/**
 * Handles DataTable pagination events.
 * Maps PrimeVue's 0-indexed page to the API's 1-indexed page.
 */
const onPage = (event: DataTablePageEvent) => {
  exampleStore.fetchExamples({
    page: event.page !== undefined ? event.page + 1 : 1,
    page_size: event.rows,
  })
}

/**
 * Handles DataTable sorting events.
 * Synchronizes sortField and sortOrder with the backend query state.
 */
const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder()
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc')
  }

  exampleStore.fetchExamples({
    sort: builder.build().sort,
    sort_by: event.sortField as string,
    is_descending: event.sortOrder === -1,
    page: 1,
  })
}

/**
 * Triggers a filtered search based on current filter values.
 * Collects values from the PrimeVue 'filters' reactive object and resets to page 1.
 */
const onFilter = () => {
  const globalFilter = filters.value.global as { value: string | null }
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] }
  const priceFilter = filters.value.price as {
    constraints: { value: number | null; matchMode: string }[]
  }
  const statusFilter = filters.value.status as { constraints: { value: ExampleStatus[] | null }[] }
  const categoryFilter = filters.value.category_name as {
    constraints: { value: string | null }[]
  }

  const builder = new QueryBuilder()

  // Handle Name Constraints
  nameFilter.constraints.forEach((c) => {
    if (c.value) builder.where('Name', '*', c.value)
  })

  // Handle Price Range (supports multiple rules like >= and <=)
  priceFilter.constraints.forEach((c) => {
    if (c.value !== null) {
      let op: FilterOperator = '='
      if (c.matchMode === FilterMatchMode.GREATER_THAN_OR_EQUAL_TO) op = '>='
      else if (c.matchMode === FilterMatchMode.LESS_THAN_OR_EQUAL_TO) op = '<='
      else if (c.matchMode === FilterMatchMode.GREATER_THAN) op = '>'
      else if (c.matchMode === FilterMatchMode.LESS_THAN) op = '<'
      else if (c.matchMode === FilterMatchMode.NOT_EQUALS) op = '!='

      builder.where('Price', op, c.value)
    }
  })

  const selectedStatuses = statusFilter.constraints[0]?.value
  if (selectedStatuses && selectedStatuses.length > 0) {
    builder.startGroup()
    selectedStatuses.forEach((s, index) => {
      if (index > 0) builder.or()
      builder.where('Status', '=', s)
    })
    builder.endGroup()
  }

  const selectedCategoryId = categoryFilter.constraints[0]?.value
  if (selectedCategoryId) {
    builder.where('CategoryId', '=', selectedCategoryId)
  }

  const built = builder.build()

  exampleStore.fetchExamples({
    search: globalFilter.value || undefined,
    search_field: globalFilter.value ? ['Name', 'Description'] : undefined,
    filter: built.filter,
    page: 1,
  })
}

/**
 * Resets all filters to their initial state and refreshes the list.
 */
const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    name: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
    },
    price: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO }],
    },
    status: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.IN }],
    },
    category_name: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    },
  }
  onFilter()
}

/**
 * Navigates to the edit form for a specific item.
 */
const editExample = (id: string) => {
  router.push({ name: 'testing.examples.edit', params: { id } })
}

/**
 * Creates a duplicate of an existing item.
 * @param id The unique identifier of the item to duplicate.
 */
const duplicateExample = async (id: string) => {
  const result = await exampleStore.duplicateExample(id)
  if (result.success) {
    showToast(
      'success',
      exampleLocales.common?.success || 'Success',
      'Item duplicated successfully',
    )
  }
}

/**
 * Shows a confirmation dialog before deleting an item.
 * Uses the localized confirm object for header and message content.
 */
const confirmDelete = (Example: ExampleListItem) => {
  confirm.require({
    message: (exampleLocales.confirm!.delete_message as (name: string) => string)(Example.name),
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
      const result = await exampleStore.deleteExample(Example.id)
      if (result.success) {
        showToast(
          'success',
          exampleLocales.common?.success || 'Deleted',
          exampleLocales.messages?.delete_success || 'Example has been removed.',
        )
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
    <AppBreadcrumb :locales="exampleLocales" />
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
          {{ exampleLocales.titles.list }}
        </h2>
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
          @click="router.push({ name: 'testing.examples.create' })"
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
        :rows="query.page_size || 10"
        :first="((query.page || 1) - 1) * (query.page_size || 10)"
        :sortField="query.sort_by"
        :sortOrder="query.is_descending ? -1 : 1"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
        class="overflow-hidden border rounded-lg shadow-sm border-surface-100 dark:border-surface-800"
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText
                v-model="(filters.global as any).value"
                :placeholder="exampleLocales.placeholders?.search"
                @keyup.enter="onFilter"
                class="w-full rounded-xl"
              />
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
              <Image
                v-if="slotProps.data.image_url"
                :src="slotProps.data.image_url"
                :alt="slotProps.data.name"
                preview
                class="w-full h-full"
                imageClass="object-cover w-full h-full transition-transform group-hover:scale-110"
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

        <Column
          field="name"
          :header="exampleLocales.table?.name"
          sortable
          :filterMatchModeOptions="stringFilterOptions"
          :showFilterOperator="false"
          :showAddButton="false"
        >
          <template #body="slotProps">
            <div class="flex flex-col">
              <span class="font-bold text-surface-900 dark:text-surface-0">{{
                slotProps.data.name
              }}</span>
              <span
                class="text-[10px] truncate text-surface-500 dark:text-surface-400 max-w-50 cursor-pointer hover:text-primary hover:underline transition-colors font-mono"
                @click="editExample(slotProps.data.id)"
                v-tooltip.bottom="'Click to Edit'"
              >
                ID: {{ slotProps.data.id }}
              </span>
            </div>
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @keyup.enter="filterCallback()"
              class="p-column-filter"
              :placeholder="exampleLocales.table?.filter_placeholder"
            />
          </template>
        </Column>

        <Column field="description" :header="exampleLocales.table?.details" class="max-w-xs">
          <template #body="slotProps">
            <p class="text-sm italic text-surface-500 dark:text-surface-400 line-clamp-1">
              {{ slotProps.data.description || exampleLocales.table?.no_details }}
            </p>
          </template>
        </Column>

        <Column
          field="category_name"
          :header="exampleLocales.labels?.category"
          class="w-32"
          :showFilterMatchModes="false"
          :showFilterOperator="false"
          :showAddButton="false"
        >
          <template #body="slotProps">
            <span class="text-sm font-medium text-surface-600 dark:text-surface-300">
              {{ slotProps.data.category_name || '-' }}
            </span>
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <Select
              v-model="filterModel.value"
              :options="categories"
              optionLabel="name"
              optionValue="id"
              :placeholder="exampleLocales.placeholders?.category || 'Select Category'"
              @change="filterCallback()"
              class="p-column-filter"
              style="min-width: 12rem"
              :showClear="true"
            />
          </template>
        </Column>

        <Column field="hex_color" :header="exampleLocales.labels?.color" class="w-24">
          <template #body="slotProps">
            <div v-if="slotProps.data.hex_color" class="flex items-center gap-2">
              <div
                class="w-4 h-4 border rounded-full shadow-sm border-surface-200"
                :style="{
                  backgroundColor: slotProps.data.hex_color.startsWith('#')
                    ? slotProps.data.hex_color
                    : '#' + slotProps.data.hex_color,
                }"
              ></div>
              <span class="text-[10px] font-mono text-surface-500">{{
                slotProps.data.hex_color.startsWith('#')
                  ? slotProps.data.hex_color
                  : '#' + slotProps.data.hex_color
              }}</span>
            </div>
          </template>
        </Column>

        <Column
          field="price"
          :header="exampleLocales.table?.price"
          sortable
          dataType="numeric"
          :filterMatchModeOptions="numericFilterOptions"
          :showFilterOperator="true"
          :showAddButton="true"
          :maxConstraints="2"
        >
          <template #body="slotProps">
            <div class="flex flex-col">
              <span class="font-black text-surface-900 dark:text-surface-0">
                {{ formatCurrency(slotProps.data.price) }}
              </span>
              <span class="text-[10px] text-primary font-bold uppercase">{{
                exampleLocales.table?.in_stock
              }}</span>
            </div>
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <InputNumber
              v-model="filterModel.value"
              mode="currency"
              currency="USD"
              locale="en-US"
              @keyup.enter="filterCallback()"
              class="w-full"
            />
          </template>
        </Column>

        <Column
          field="status"
          :header="exampleLocales.table?.status"
          class="w-32"
          :showFilterMatchModes="false"
          :showFilterOperator="false"
          :showAddButton="false"
        >
          <template #body="slotProps">
            <Badge
              :value="
                (exampleLocales.labels &&
                  exampleLocales.labels[
                    `status_${ExampleStatus[slotProps.data.status as ExampleStatus].toLowerCase()}`
                  ]) ||
                ExampleStatus[slotProps.data.status as ExampleStatus]
              "
              :severity="STATUS_COLORS[slotProps.data.status as ExampleStatus].severity"
              class="px-3 py-1 font-bold rounded-full"
            />
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <MultiSelect
              v-model="filterModel.value"
              :options="statuses"
              optionLabel="label"
              optionValue="value"
              :placeholder="exampleLocales.placeholders?.select_status || 'Select Status'"
              @change="filterCallback()"
              class="p-column-filter"
              style="min-width: 12rem"
              :showClear="true"
            >
              <template #option="slotProps">
                <Badge
                  :value="slotProps.option.label"
                  :severity="STATUS_COLORS[slotProps.option.value as ExampleStatus].severity"
                ></Badge>
              </template>
            </MultiSelect>
          </template>
        </Column>

        <Column
          :header="exampleLocales.table?.actions"
          class="w-40 text-right"
          frozen
          alignFrozen="right"
        >
          <template #body="slotProps">
            <div class="flex justify-end gap-1">
              <Button
                icon="pi pi-copy"
                severity="secondary"
                text
                rounded
                @click="duplicateExample(slotProps.data.id)"
                v-tooltip.top="'Duplicate'"
              />
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

/* Force PrimeVue Image to fill the table cell */
:deep(.p-image),
:deep(.p-image > img) {
  width: 100%;
  height: 100%;
  display: block;
}
</style>
