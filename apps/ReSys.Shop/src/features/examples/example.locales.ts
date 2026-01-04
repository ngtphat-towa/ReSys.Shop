import type { FeatureLocales } from '@/shared/locales/locale.types';

/**
 * Feature-specific strings for Examples in Shop
 * Implements FeatureLocales for consistent structure
 */
export const exampleLocales: FeatureLocales = {
    titles: {
        list: 'Exclusive Collections',
        detail: 'Product Details'
    },
    descriptions: {
        list: 'Find the perfect items curated just for your lifestyle.'
    },
    labels: {
        price: 'Price',
        details: 'The Details',
        status: 'Status',
        shipping: 'Fast Shipping',
        secure: 'Secure Store'
    },
    table: {
        in_stock: 'In Stock',
        active: 'Active'
    },
    placeholders: {
        search: 'Search our catalog...'
    },
    messages: {
        loading: 'Loading...',
        no_results: 'No results found',
        no_results_desc: "We couldn't find anything matching your search. Try another term!",
        not_found: 'Example Not Found',
        not_found_desc: "The item you're looking for might have been moved or is no longer available.",
        premium: 'Premium Quality',
        shipping_desc: 'Arrives in 2-3 days',
        secure_desc: '100% money back'
    },
    actions: {
        view_all: 'View All Examples',
        add_to_bag: 'Add to Shopping Bag',
        details: 'Details',
        add: 'Add',
        return: 'Return to Shop'
    }
};
