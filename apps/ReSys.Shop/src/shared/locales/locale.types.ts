/**
 * Base schema for feature-specific locales.
 * Used to ensure all feature modules provide consistent UI strings across the Shop catalog.
 */
export interface FeatureLocales {
    /** Primary headers and breadcrumb titles. */
    titles: Record<string, string>;
    /** Explanatory sub-text for pages or sections. */
    descriptions?: Record<string, string>;
    /** Form field labels. */
    labels?: Record<string, string>;
    /** Input hints and placeholder text. */
    placeholders?: Record<string, string>;
    /** Contextual help strings. */
    tooltips?: Record<string, string>;
    /** Feedback messages for user actions. */
    messages: Record<string, string>;
    /** Button and clickable action labels. */
    actions: Record<string, string>;
    /** Grid headers and metadata strings. */
    table?: Record<string, string>;
    /** Configuration for confirmation dialogs. */
    confirm?: Record<string, string | ((...args: any[]) => string)>;
}

/**
 * General/Common strings used across the entire Shop application.
 * Contains strings that are independent of any specific business feature.
 */
export interface GeneralLocales {
    /** Basic action labels (Save, Cancel, etc.) */
    common: {
        confirm: string;
        cancel: string;
        save: string;
        delete: string;
        edit: string;
        back: string;
        next: string;
        prev: string;
        yes: string;
        no: string;
        loading: string;
        success: string;
        error: string;
        warning: string;
        info: string;
    };
    /** Top-level navigation labels. */
    navigation: {
        dashboard: string;
        home: string;
        profile: string;
        settings: string;
        logout: string;
    };
}
