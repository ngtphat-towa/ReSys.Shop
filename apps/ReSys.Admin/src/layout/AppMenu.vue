<script setup lang="ts">
import { ref } from 'vue';
import AppMenuItem from './AppMenuItem.vue';
import type { MenuItem } from './AppMenuItem.vue';

const model = ref<MenuItem[]>([
    {
        label: 'Home',
        items: [{ label: 'Dashboard', icon: 'pi pi-fw pi-home', to: '/' }]
    },
    {
        label: 'Catalog',
        items: [
            { label: 'Products', icon: 'pi pi-fw pi-shopping-bag', to: '/catalog/products' }
        ]
    },
    {
        label: 'Testing',
        items: [
            { 
                label: 'Examples', 
                icon: 'pi pi-fw pi-database', 
                items: [
                    { label: 'Catalog List', icon: 'pi pi-fw pi-list', to: '/Examples' },
                    { label: 'Add New', icon: 'pi pi-fw pi-plus-circle', to: '/Examples/create' }
                ]
            }
        ]
    }
]);
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item.label">
            <app-menu-item v-if="!item.separator" :item="item" :index="i" :root="true"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
