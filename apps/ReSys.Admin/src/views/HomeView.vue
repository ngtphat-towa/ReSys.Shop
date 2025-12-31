<script setup lang="ts">
import { ref } from 'vue';
import Card from 'primevue/card';

const stats = ref([
    { title: 'Total Sales', value: '$12,345', icon: 'pi pi-dollar', color: 'text-green-500' },
    { title: 'Total Orders', value: '150', icon: 'pi pi-shopping-cart', color: 'text-blue-500' },
    { title: 'New Customers', value: '45', icon: 'pi pi-users', color: 'text-yellow-500' },
    { title: 'Products', value: '86', icon: 'pi pi-box', color: 'text-purple-500' }
]);
</script>

<template>
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <Card v-for="stat in stats" :key="stat.title">
            <template #title>
                <div class="flex justify-between items-center">
                    <span class="text-sm font-medium text-gray-500 uppercase">{{ stat.title }}</span>
                    <i :class="[stat.icon, stat.color, 'text-xl']"></i>
                </div>
            </template>
            <template #content>
                <div class="text-2xl font-bold">{{ stat.value }}</div>
            </template>
        </Card>
    </div>

    <Card>
        <template #title>Recent Activity</template>
        <template #content>
            <p class="text-gray-600">No recent activity to show.</p>
        </template>
    </Card>
</template>