<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import { generalLocales } from '@/shared/locales/general.locales';

/**
 * Props for the breadcrumb component.
 * @property locales Optional feature-specific locales to resolve keys against.
 */
interface Props {
  locales?: Record<string, unknown>;
}

const props = defineProps<Props>();
const route = useRoute();

/**
 * Resolves a dot-notation key (e.g., 'navigation.home') against
 * feature locales first, then general locales.
 */
const resolveLabel = (key: string): string => {
  if (!key) return '';
  
  const parts = key.split('.');
  
  // 1. Try resolving against feature locales (passed via props)
  let current: unknown = props.locales;
  for (const part of parts) {
    if (current && typeof current === 'object' && part in (current as Record<string, unknown>)) {
      current = (current as Record<string, unknown>)[part];
    } else {
      current = null;
      break;
    }
  }
  
  if (current && typeof current === 'string') return current;

  // 2. Try resolving against general locales
  current = generalLocales;
  for (const part of parts) {
    if (current && typeof current === 'object' && part in (current as Record<string, unknown>)) {
      current = (current as Record<string, unknown>)[part];
    } else {
      current = null;
      break;
    }
  }

  // Fallback to the key itself if resolution fails
  return typeof current === 'string' ? current : key;
};

/**
 * Computes the breadcrumb list based on the matched route hierarchy.
 */
const breadcrumbs = computed(() => {
  // Get all matched route segments that have a breadcrumb defined in meta
  const matched = route.matched.filter(r => r.meta && r.meta.breadcrumb);
  
  return matched.map((record, index) => {
    // Determine the path: if the route has children and the path is empty (index route), 
    // it might not be the link we want. 
    const path = record.path || '/';
    
    return {
      label: resolveLabel(record.meta.breadcrumb as string),
      to: record.name ? { name: record.name } : path,
      // Active if it's the last segment
      active: index === matched.length - 1
    };
  });
});
</script>

<template>
  <nav v-if="breadcrumbs.length > 0" class="flex mb-6 text-sm" aria-label="Breadcrumb">
    <ol class="inline-flex items-center space-x-1 md:space-x-3">
      <li v-for="(item, index) in breadcrumbs" :key="index" class="inline-flex items-center">
        <div class="flex items-center">
          <!-- Chevron separator for non-first items -->
          <i v-if="index > 0" class="pi pi-chevron-right text-surface-400 mx-2 text-[10px]"></i>
          
          <!-- Link for non-active items -->
          <router-link
            v-if="!item.active"
            :to="item.to"
            class="transition-colors text-surface-500 hover:text-primary dark:text-surface-400 flex items-center"
          >
            <i v-if="index === 0" class="mr-2 pi pi-home text-[12px]"></i>
            {{ item.label }}
          </router-link>
          
          <!-- Static text for the current active page -->
          <span v-else class="font-bold text-primary flex items-center">
            <i v-if="index === 0" class="mr-2 pi pi-home text-[12px]"></i>
            {{ item.label }}
          </span>
        </div>
      </li>
    </ol>
  </nav>
</template>
