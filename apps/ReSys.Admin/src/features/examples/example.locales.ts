/**
 * Feature-specific strings for Examples
 * Organized to make future i18n migration easy
 */
export const ExampleLocales = {
    titles: {
        list: 'Example Catalog',
        create: 'Create New Product',
        edit: 'Update Product',
        breadcrumb_home: 'Dashboard',
        breadcrumb_parent: 'Examples'
    },
    descriptions: {
        list: 'Manage your store inventory and Example details.'
    },
    labels: {
        name: 'Example Name',
        description: 'Description',
        price: 'Price (USD)',
        category: 'Category',
        media: 'Product Media',
        summary: 'Publish Summary'
    },
    table: {
        preview: 'Preview',
        name: 'Name',
        details: 'Details',
        price: 'Price',
        status: 'Status',
        actions: 'Actions',
        no_details: 'No additional details',
        active: 'Active',
        in_stock: 'In Stock'
    },
    placeholders: {
        search: 'Search by name...',
        name: 'e.g. Premium Wireless Headphones',
        description: 'Provide a detailed description of the Example features...',
        upload: 'Upload New Image'
    },
    tooltips: {
        name: 'The public name of the product as it appears in the catalog.',
        description: 'A detailed summary of features and benefits. Supports up to 2000 characters.',
        price: 'Set the retail price in USD. Minimum value is $0.01.',
        media: 'Upload a high-quality JPG or PNG. Max size 1MB.',
        edit: 'Edit item',
        delete: 'Delete item'
    },
    confirm: {
        delete_header: 'Dangerous Action',
        delete_message: (name: string) => `Are you sure you want to delete "${name}"? This action cannot be undone.`,
        reject_label: 'Cancel',
        accept_label: 'Delete'
    },
    messages: {
        create_success: 'New Example has been successfully added.',
        update_success: 'Example has been successfully updated.',
        validation_failed: 'Please correct the errors before saving.',
        loading: 'Loading...',
        no_image: 'No image selected',
        empty_list: 'No Examples found'
    },
    actions: {
        cancel: 'Cancel',
        save_create: 'Create Product',
        save_edit: 'Update Changes',
        new: 'New Example'
    }
};
