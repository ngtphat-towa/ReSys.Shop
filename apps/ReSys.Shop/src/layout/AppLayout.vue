<script setup lang="ts">
import { useLayout } from '@/layout/composables/layout';
import { computed, watch, ref } from 'vue';
import { useToast } from 'primevue/usetoast';
import { toastBus } from '@/shared/api/api-client';
import AppTopbar from './AppTopbar.vue';
import AppFooter from './AppFooter.vue';

const { layoutConfig, layoutState, hideMobileMenu } = useLayout();
const toast = useToast();

watch(toastBus, (newValue) => {
    if (newValue) {
        toast.add(newValue);
        toastBus.value = null;
    }
});

const containerClass = computed(() => {
    return {
        'layout-overlay': layoutConfig.menuMode === 'overlay',
        'layout-static': layoutConfig.menuMode === 'static',
        'layout-static-inactive': layoutState.staticMenuInactive && layoutConfig.menuMode === 'static',
        'layout-overlay-active': layoutState.overlayMenuActive,
        'layout-mobile-active': layoutState.mobileMenuActive,
    };
});

const outsideClickListener = ref<((event: MouseEvent) => void) | null>(null);

watch(() => layoutState.mobileMenuActive, (newVal) => {
    if (newVal) {
        bindOutsideClickListener();
    } else {
        unbindOutsideClickListener();
    }
});

const bindOutsideClickListener = () => {
    if (!outsideClickListener.value) {
        outsideClickListener.value = (event: MouseEvent) => {
            if (isOutsideClicked(event)) {
                hideMobileMenu();
            }
        };
        document.addEventListener('click', outsideClickListener.value);
    }
};

const unbindOutsideClickListener = () => {
    if (outsideClickListener.value) {
        document.removeEventListener('click', outsideClickListener.value);
        outsideClickListener.value = null;
    }
};

const isOutsideClicked = (event: MouseEvent) => {
    const sidebarEl = document.querySelector('.layout-sidebar');
    const topbarEl = document.querySelector('.layout-menu-button');

    return !(sidebarEl?.isSameNode(event.target as Node) || sidebarEl?.contains(event.target as Node) || topbarEl?.isSameNode(event.target as Node) || topbarEl?.contains(event.target as Node));
};
</script>

<template>
    <div class="layout-wrapper" :class="containerClass">
        <AppTopbar />
        <div class="layout-main-container">
            <div class="layout-main">
                <router-view />
            </div>
            <AppFooter />
        </div>
        <div class="layout-mask animate-fadein" @click="hideMobileMenu" />
    </div>
    <Toast class="layout-toast" />
    <ConfirmDialog />
</template>

<style lang="scss" scoped>
/* Position toast below the top bar */
:global(.p-toast.p-component.p-toast-top-right) {
    top: 5rem;
}

/* For shop, we don't have a sidebar usually, so we adjust main padding */
.layout-main-container {
    margin-left: 0 !important;
    padding-top: 5rem;
}
</style>