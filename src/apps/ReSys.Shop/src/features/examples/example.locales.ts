import type { FeatureLocales } from '@/shared/locales/locale.types';

/**
 * Feature-specific strings for Examples in Shop
 * Implements FeatureLocales for consistent structure
 */
export const exampleLocales: FeatureLocales = {
    titles: {
        list: 'Exclusive Collections',
        detail: 'Product Details',
        hero_primary: 'Exclusive',
        hero_highlight: 'Collections',
        breadcrumb_home: 'Shop',
        breadcrumb_parent: 'Catalog',
        recommendations_primary: 'You May Also',
        recommendations_highlight: 'Love',
        recommendations_desc: 'Curated recommendations based on your current choice.'
    },
    descriptions: {
        list: 'Find the perfect items curated just for your lifestyle.',
        hero: 'Find the perfect items curated just for your lifestyle.'
    },
    labels: {
        price: 'Price',
        details: 'The Details',
        status: 'Status',
        shipping: 'Fast Shipping',
        secure: 'Secure Store',
        rating_in_stock: 'In Stock & Ready to ship',
        free_delivery: 'Free express delivery on this item.'
    },
    table: {
        in_stock: 'In Stock',
        active: 'Active',
        no_details: 'No description available'
    },
    placeholders: {
        search: 'Search our catalog...'
    },
    messages: {
        loading: 'Loading...',
        empty_list: 'No results found',
        empty_description: "We couldn't find anything matching your search. Try another term!",
        not_found: 'Example Not Found',
        not_found_desc: "The item you're looking for might have been moved or is no longer available.",
        premium: 'Premium Quality',
        shipping_desc: 'Arrives in 2-3 days',
        secure_desc: '100% money back',
        no_details_desc: 'No detailed description available for this item.'
    },
    actions: {
        view_all: 'View All Examples',
        add_to_bag: 'Add to Shopping Bag',
        details: 'Details',
        add: 'Add',
        return: 'Return to Shop',
        back_to_catalog: 'Back to Catalog',
        explore_all: 'Explore All'
    }
};
