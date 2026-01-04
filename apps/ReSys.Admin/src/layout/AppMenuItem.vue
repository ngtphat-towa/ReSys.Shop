<script setup lang="ts">
import { useLayout } from '@/layout/composables/layout';
import { computed, ref, watch } from 'vue';
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
}

const route = useRoute();
const { layoutState, isDesktop } = useLayout();

const props = defineProps<{
    item: MenuItem;
    index?: number;
    root?: boolean;
}>();

const active = ref(false);

const isActive = computed(() => {
    if (props.item.to && route.path === props.item.to) return true;
    if (props.item.items) {
        return props.item.items.some(child => {
            if (child.to === route.path) return true;
            if (child.items) return child.items.some(sub => sub.to === route.path);
            return false;
        });
    }
    return false;
});

watch(() => route.path, (newPath) => {
    if (props.item.items) {
        const hasActiveChild = props.item.items.some(child => {
            if (child.to === newPath) return true;
            if (child.items) return child.items.some(sub => sub.to === newPath);
            return false;
        });
        if (hasActiveChild) active.value = true;
    }
}, { immediate: true });

const itemClick = (event: Event, item: MenuItem) => {
    if (item.disabled) {
        event.preventDefault();
        return;
    }

    if (item.command) {
        item.command({ originalEvent: event, item: item });
    }

    if (item.items) {
        active.value = !active.value;
    }

    if (item.to || item.url) {
        layoutState.overlayMenuActive = false;
        layoutState.mobileMenuActive = false;
    }
};
</script>

<template>
    <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': active || isActive }">
        <!-- 1. Root Header Label (Only shows for top-level categories) -->
        <div v-if="root && item.visible !== false" class="layout-menuitem-root-text">
            {{ item.label }}
        </div>
        
        <!-- 2. Parent Link (NOT a root, but HAS children - e.g., Examples) -->
        <a v-if="!root && item.items && item.visible !== false" 
           :href="item.url" 
           @click="itemClick($event, item)" 
           :class="[item.class, { 'active-route': isActive }]" 
           :target="item.target" 
           tabindex="0"
        >
            <i v-if="item.icon" :class="item.icon" class="layout-menuitem-icon" />
            <span class="layout-menuitem-text">{{ item.label }}</span>
            <i class="pi pi-fw pi-angle-down layout-submenu-toggler" />
        </a>

        <!-- 3. Leaf Link (NOT a root, NO children - e.g., Dashboard, List, Create) -->
        <router-link v-if="!root && item.to && !item.items && item.visible !== false" 
                     @click="itemClick($event, item)" 
                     exactActiveClass="active-route" 
                     :class="[item.class]" 
                     tabindex="0" 
                     :to="item.to"
        >
            <i v-if="item.icon" :class="item.icon" class="layout-menuitem-icon" />
            <span class="layout-menuitem-text">{{ item.label }}</span>
        </router-link>

        <!-- 4. Recursive Submenu -->
        <Transition v-if="item.items && item.visible !== false" name="layout-submenu">
            <ul v-show="root ? true : active || isActive" class="layout-submenu">
                <app-menu-item v-for="(child, i) in item.items" :key="child.label + '_' + i" :item="child" :index="i" :root="false" />
            </ul>
        </Transition>
    </li>
</template>
