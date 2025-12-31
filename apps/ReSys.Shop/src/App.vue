<script setup lang="ts">
import { RouterView } from 'vue-router';
import Toast from 'primevue/toast';
import { useToast } from 'primevue/usetoast';
import { watch } from 'vue';
import { toastBus } from '@/services/apiClient';

const toast = useToast();

watch(toastBus, (newValue) => {
  if (newValue) {
    toast.add(newValue);
    toastBus.value = null;
  }
});
</script>

<template>
  <Toast />
  <RouterView />
</template>