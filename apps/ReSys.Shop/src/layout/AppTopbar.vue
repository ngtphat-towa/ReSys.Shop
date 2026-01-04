<script setup lang="ts">
import { useLayout } from '@/layout/composables/layout';
import { useRouter } from 'vue-router';

const { toggleDarkMode, isDarkTheme } = useLayout();
const router = useRouter();
</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <router-link to="/" class="layout-topbar-logo">
                <svg width="35" height="40" viewBox="0 0 35 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M25.87 18.05L23.16 14.84L21.02 12.31L0.460022 36.63L2.61002 39.18L34.1 1.93L31.95 -0.620003L25.87 18.05Z" fill="var(--primary-color)" />
                    <path d="M19.14 26.03L16.43 22.82L14.29 20.29L3.58 33L5.73 35.55L19.14 19.66V26.03Z" fill="var(--primary-color)" />
                </svg>
                <span>RESYS SHOP</span>
            </router-link>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-topbar-menu hidden lg:flex gap-6 mr-8">
                <router-link to="/" class="text-surface-600 dark:text-surface-300 font-bold hover:text-primary transition-colors">Home</router-link>
                <router-link to="/about" class="text-surface-600 dark:text-surface-300 font-bold hover:text-primary transition-colors">About</router-link>
            </div>

            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
                    <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
                </button>
                <button type="button" class="layout-topbar-action">
                    <i class="pi pi-shopping-bag"></i>
                </button>
                <button type="button" class="layout-topbar-action">
                    <i class="pi pi-user"></i>
                </button>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.layout-topbar {
    padding: 0 4rem;
}
@media (max-width: 991px) {
    .layout-topbar {
        padding: 0 2rem;
    }
}
</style>
