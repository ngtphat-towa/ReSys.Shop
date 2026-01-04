<script setup lang="ts">
import { RouterView } from 'vue-router';
import Menubar from 'primevue/menubar';
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const items = ref([
    {
        label: 'Home',
        icon: 'pi pi-home',
        command: () => {
            router.push('/');
        }
    },
    {
        label: 'Examples',
        icon: 'pi pi-shopping-bag',
        command: () => {
            // router.push('/Examples'); // Future
        }
    }
]);
</script>

<template>
    <div class="min-h-screen flex flex-col bg-slate-50">
        <header class="sticky top-0 z-50 shadow-sm">
            <Menubar :model="items" class="border-none rounded-none px-4 lg:px-8">
                <template #start>
                    <div class="text-xl font-bold text-primary mr-4 cursor-pointer" @click="router.push('/')">
                        ReSys Shop
                    </div>
                </template>
                <template #end>
                    <div class="flex items-center gap-2">
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <input type="text" placeholder="Search..." class="p-inputtext p-component p-inputtext-sm" />
                        </span>
                    </div>
                </template>
            </Menubar>
        </header>

        <main class="flex-grow container mx-auto px-4 py-8">
            <RouterView />
        </main>

        <footer class="bg-slate-900 text-white py-8 mt-auto">
            <div class="container mx-auto px-4 text-center">
                <p>&copy; 2025 ReSys Shop. All rights reserved.</p>
            </div>
        </footer>
    </div>
</template>

<style scoped>
:deep(.p-menubar) {
    background-color: rgba(255, 255, 255, 0.9);
    backdrop-filter: blur(8px);
}
</style>
