/**
 * Base schema for feature-specific locales
 * This mirrors the structure required by vue-i18n
 */
export interface FeatureLocales {
    titles: Record<string, string>;
    descriptions?: Record<string, string>;
    labels?: Record<string, string>;
    placeholders?: Record<string, string>;
    tooltips?: Record<string, string>;
    messages: Record<string, string>;
    actions: Record<string, string>;
    table?: Record<string, string>;
    confirm?: Record<string, string | ((...args: any[]) => string)>;
}

/**
 * General/Common strings used across the entire application
 */
export interface GeneralLocales {
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
    navigation: {
        dashboard: string;
        home: string;
        profile: string;
        settings: string;
        logout: string;
    };
}
