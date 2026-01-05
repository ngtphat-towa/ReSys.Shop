<script setup lang="ts">
import { RouterView } from 'vue-router';
import { watch } from 'vue';
import { useToast } from 'primevue/usetoast';
import { toastBus } from '@/shared/composables/toast.use';

const toast = useToast();

watch(toastBus, (newValue) => {
  if (newValue) {
    toast.add(newValue);
    toastBus.value = null;
  }
});
</script>

<template>
    <router-view />
</template>