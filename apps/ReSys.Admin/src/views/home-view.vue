<script setup lang="ts">
import { ref } from 'vue';

const stats = ref([
    { title: 'Orders', value: '152', icon: 'pi pi-shopping-cart', color: 'blue', trend: '24 new', trendColor: 'text-primary' },
    { title: 'Revenue', value: '$2,100', icon: 'pi pi-map-marker', color: 'orange', trend: '%52+', trendColor: 'text-primary' },
    { title: 'Customers', value: '28441', icon: 'pi pi-inbox', color: 'cyan', trend: '520', trendColor: 'text-primary' },
    { title: 'Comments', value: '152 Unread', icon: 'pi pi-comment', color: 'purple', trend: '85', trendColor: 'text-primary' }
]);
</script>

<template>
    <div class="grid grid-cols-12 gap-4">
        <div class="col-span-12 mb-4">
            <h1 class="text-3xl font-black text-surface-900 dark:text-surface-0 tracking-tight">Dashboard Overview</h1>
            <p class="text-surface-500 dark:text-surface-400 mt-1">Welcome back! Here's what's happening today.</p>
        </div>

        <div v-for="stat in stats" :key="stat.title" class="col-span-12 lg:col-span-6 xl:col-span-3">
            <div class="card mb-0 bg-surface-0 dark:bg-surface-900 p-4 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm h-full">
                <div class="flex justify-between mb-4">
                    <div>
                        <span class="block text-surface-500 dark:text-surface-400 font-medium mb-4 uppercase text-xs tracking-wider">{{ stat.title }}</span>
                        <div class="text-surface-900 dark:text-surface-0 font-bold text-2xl">{{ stat.value }}</div>
                    </div>
                    <div class="flex items-center justify-center rounded-xl" :class="`bg-${stat.color}-100/50 dark:bg-${stat.color}-900/20`" style="width: 2.5rem; height: 2.5rem">
                        <i :class="[stat.icon, `text-${stat.color}-500`, 'text-xl']"></i>
                    </div>
                </div>
                <span :class="[stat.trendColor, 'font-bold']">{{ stat.trend }} </span>
                <span class="text-surface-500 dark:text-surface-400">since last visit</span>
            </div>
        </div>

        <div class="col-span-12 xl:col-span-6 mt-4">
            <div class="card bg-surface-0 dark:bg-surface-900 p-6 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm">
                <div class="flex justify-between items-center mb-6">
                    <h5 class="text-xl font-bold text-surface-900 dark:text-surface-0">Recent Activity</h5>
                    <Button icon="pi pi-ellipsis-v" text rounded severity="secondary"></Button>
                </div>
                <ul class="list-none p-0 m-0">
                    <li class="flex items-center py-3 border-b border-surface-100 dark:border-surface-800 last:border-0">
                        <div class="w-10 h-10 flex items-center justify-center bg-primary/10 rounded-full mr-4">
                            <i class="pi pi-dollar text-primary"></i>
                        </div>
                        <div class="flex flex-col">
                            <span class="text-surface-900 dark:text-surface-0 font-medium">Richard Jones</span>
                            <span class="text-surface-500 dark:text-surface-400 text-sm">Bought a Blue T-Shirt for $79</span>
                        </div>
                        <span class="ml-auto text-surface-400 text-xs">2m ago</span>
                    </li>
                    <li class="flex items-center py-3 border-b border-surface-100 dark:border-surface-800 last:border-0">
                        <div class="w-10 h-10 flex items-center justify-center bg-primary/10 rounded-full mr-4">
                            <i class="pi pi-download text-primary"></i>
                        </div>
                        <div class="flex flex-col">
                            <span class="text-surface-900 dark:text-surface-0 font-medium">System Update</span>
                            <span class="text-surface-500 dark:text-surface-400 text-sm">New version 2.4.0 is available</span>
                        </div>
                        <span class="ml-auto text-surface-400 text-xs">1h ago</span>
                    </li>
                </ul>
            </div>
        </div>

        <div class="col-span-12 xl:col-span-6 mt-4">
            <div class="card bg-surface-0 dark:bg-surface-900 p-6 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm h-full">
                <h5 class="text-xl font-bold text-surface-900 dark:text-surface-0 mb-6">Quick Actions</h5>
                <div class="grid grid-cols-2 gap-4">
                    <Button label="Add Example" icon="pi pi-plus" class="p-4 flex flex-col items-center gap-2" outlined @click="$router.push({ name: 'testing.examples.create' })"></Button>
                    <Button label="View Catalog" icon="pi pi-list" severity="secondary" class="p-4 flex flex-col items-center gap-2" outlined @click="$router.push({ name: 'testing.examples.list' })"></Button>
                </div>
            </div>
        </div>
    </div>
</template>
