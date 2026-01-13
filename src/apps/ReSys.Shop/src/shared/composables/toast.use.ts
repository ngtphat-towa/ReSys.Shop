import { ref } from 'vue';

export interface ToastMessage {
  severity: 'success' | 'info' | 'warn' | 'error';
  summary: string;
  detail: string;
  life?: number;
}

/**
 * Global Event Bus for Notifications.
 * Components (like App.vue) watch this ref to display toasts.
 */
export const toastBus = ref<ToastMessage | null>(null);

/**
 * Composable for triggering toast notifications.
 */
export function useToast() {
  const showToast = (severity: ToastMessage['severity'], summary: string, detail: string, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
  };

  return {
    showToast,
    toastBus
  };
}
