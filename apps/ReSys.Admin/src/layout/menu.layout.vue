<script setup lang="ts">
import { ref } from 'vue';
import AppMenuItem from './menu-item.layout.vue';
import type { MenuItem } from './menu-item.layout.vue';

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
                    { label: 'Catalog List', icon: 'pi pi-fw pi-list', to: { name: 'testing.examples.list' } },
                    { label: 'Add New', icon: 'pi pi-fw pi-plus-circle', to: { name: 'testing.examples.create' } }
                ]
            }
        ]
    }
]);
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item.label">
            <AppMenuItem v-if="!item.separator" :item="item" :index="i" :root="true"></AppMenuItem>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
