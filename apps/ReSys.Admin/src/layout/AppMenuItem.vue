<script setup lang="ts">
import { useLayout } from '@/layout/composables/layout';
import { computed, ref, onBeforeMount, watch } from 'vue';
import { useRoute } from 'vue-router';

export interface MenuItem {
    label?: string;
    icon?: string;
    to?: string | object;
    url?: string;
    target?: string;
    items?: MenuItem[];
    separator?: boolean;
    visible?: boolean;
    disabled?: boolean;
    class?: string;
    command?: (event: { originalEvent: Event; item: MenuItem }) => void;
    path?: string;
}

const route = useRoute();
const { layoutState, isDesktop } = useLayout();

const props = defineProps<{
    item: MenuItem;
    index?: number;
    root?: boolean;
    parentPath?: string;
}>();

const isActive = computed(() => {
    const fullPath = props.item.path ? (props.parentPath ? props.parentPath + props.item.path : props.item.path) : null;
    return props.item.path ? layoutState.activePath?.startsWith(fullPath!) : layoutState.activePath === props.item.to;
});

const itemClick = (event: Event, item: MenuItem) => {
    if (item.disabled) {
        event.preventDefault();
        return;
    }

    if (item.command) {
        item.command({ originalEvent: event, item: item });
    }

    if (item.items) {
        if (isActive.value) {
            // Logic for closing submenu
            layoutState.activePath = layoutState.activePath?.replace(item.path || '', '') || null;
        } else {
            const fullPath = item.path ? (props.parentPath ? props.parentPath + item.path : item.path) : null;
            layoutState.activePath = fullPath || null;
            layoutState.menuHoverActive = true;
        }
    } else {
        layoutState.overlayMenuActive = false;
        layoutState.mobileMenuActive = false;
        layoutState.menuHoverActive = false;
    }
};

const onMouseEnter = () => {
    if (isDesktop() && props.root && props.item.items && layoutState.menuHoverActive) {
        const fullPath = props.item.path ? (props.parentPath ? props.parentPath + props.item.path : props.item.path) : null;
        layoutState.activePath = fullPath || null;
    }
};
</script>

<template>
    <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': isActive }">
        <div v-if="root && item.visible !== false" class="layout-menuitem-root-text">{{ item.label }}</div>
        <a v-if="(!item.to || item.items) && item.visible !== false" :href="item.url" @click="itemClick($event, item)" :class="item.class" :target="item.target" tabindex="0" @mouseenter="onMouseEnter">
            <i :class="item.icon" class="layout-menuitem-icon" />
            <span class="layout-menuitem-text">{{ item.label }}</span>
            <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items" />
        </a>
        <router-link v-if="item.to && !item.items && item.visible !== false" @click="itemClick($event, item)" exactActiveClass="active-route" :class="item.class" tabindex="0" :to="item.to" @mouseenter="onMouseEnter">
            <i :class="item.icon" class="layout-menuitem-icon" />
            <span class="layout-menuitem-text">{{ item.label }}</span>
            <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items" />
        </router-link>
        <Transition v-if="item.items && item.visible !== false" name="layout-submenu">
            <ul v-show="root ? true : isActive" class="layout-submenu">
                <app-menu-item v-for="(child, i) in item.items" :key="child.label + '_' + i" :item="child" :index="i" :root="false" :parentPath="item.path ? (parentPath ? parentPath + item.path : item.path) : parentPath" />
            </ul>
        </Transition>
    </li>
</template>
