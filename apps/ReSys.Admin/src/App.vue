<script setup lang="ts">
import { RouterView } from 'vue-router';
import { useToast } from 'primevue/usetoast';
import { watch } from 'vue';
import { toastBus } from '@/shared/api/client';

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