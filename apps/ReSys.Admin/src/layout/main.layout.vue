<script setup lang="ts">
import { useLayout } from '@/layout/composables/layout.composable';
import { computed, watch, ref } from 'vue';
import AppTopbar from './topbar.layout.vue';
import AppFooter from './footer.layout.vue';
import AppSidebar from './sidebar.layout.vue';

const { layoutConfig, layoutState, hideMobileMenu } = useLayout();

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
        <AppSidebar />
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
.layout-toast {
    :deep(.p-toast-message-content) {
        padding: 1rem;
    }
}

/* Position toast below the top bar (4rem height + some margin) */
:global(.p-toast.p-component.p-toast-top-right) {
    top: 5rem;
}

:global(.p-toast.p-component.p-toast-top-left) {
    top: 5rem;
}

:global(.p-toast.p-component.p-toast-top-center) {
    top: 5rem;
}
</style>