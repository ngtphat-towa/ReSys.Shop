import type { FeatureLocales } from '@/shared/locales/locale.types'

export const exampleCategoryLocales: FeatureLocales = {
  titles: {
    list: 'Example Categories',
    create: 'Create New Category',
    edit: 'Update Category',
    breadcrumb_home: 'Dashboard',
    breadcrumb_parent: 'Categories',
    basic_info: 'Category Information',
  },
  descriptions: {
    list: 'Manage your product categories and classifications.',
    create: 'Fill in the details to add a new category.',
  },
  labels: {
    name: 'Category Name',
    description: 'Description',
    summary: 'Category Summary',
  },
  table: {
    name: 'Name',
    details: 'Details',
    actions: 'Actions',
    no_details: 'No additional details',
    filter_placeholder: 'Search name',
    clear_filter: 'Clear Filters',
  },
  filters: {
    contains: 'Contains',
    equals: 'Equals',
    not_equals: 'Not Equals',
    starts_with: 'Starts With',
    ends_with: 'Ends With',
  },
  placeholders: {
    search: 'Search categories...',
    name: 'e.g. Electronics, Books, etc.',
    description: 'Provide a brief description of the category...',
  },
  tooltips: {
    name: 'The name of the category as it appears in the system.',
    description: 'A brief overview of what this category includes.',
    edit: 'Edit category',
    delete: 'Delete category',
  },
  confirm: {
    delete_header: 'Confirm Deletion',
    delete_message: (...args: string[]) =>
      `Are you sure you want to delete "${args[0]}"? This may affect items in this category.`,
    reject_label: 'Cancel',
    accept_label: 'Delete',
  },
  messages: {
    create_success: 'New Category has been successfully added.',
    update_success: 'Category has been successfully updated.',
    delete_success: 'Category has been successfully removed.',
    load_error: 'Failed to load category details.',
    validation_failed: 'Please correct the errors before saving.',
    loading: 'Loading...',
    modifying: 'Modifying',
    empty_list: 'No Categories found',
  },
  actions: {
    cancel: 'Cancel',
    save_create: 'Create Category',
    save_edit: 'Update Changes',
    new: 'New Category',
    create: 'Create',
    edit: 'Edit',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  },
}
