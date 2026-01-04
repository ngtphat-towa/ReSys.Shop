<script setup lang="ts">
import { RouterView } from 'vue-router';
import Menubar from 'primevue/menubar';
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const items = ref([
    {
        label: 'Dashboard',
        icon: 'pi pi-home',
        command: () => router.push('/')
    },
    {
        label: 'Examples',
        icon: 'pi pi-box',
        items: [
            {
                label: 'List',
                icon: 'pi pi-list',
                command: () => router.push('/Examples')
            },
            {
                label: 'Create',
                icon: 'pi pi-plus',
                command: () => router.push('/Examples/create')
            }
        ]
    }
]);
</script>

<template>
    <div class="min-h-screen flex flex-col bg-slate-50">
        <header class="sticky top-0 z-50 shadow-md">
            <Menubar :model="items" class="border-none rounded-none px-4">
                <template #start>
                    <div class="text-xl font-bold text-primary mr-8">
                        ReSys Admin
                    </div>
                </template>
                <template #end>
                    <div class="flex items-center gap-2">
                        <i class="pi pi-user text-xl"></i>
                    </div>
                </template>
            </Menubar>
        </header>

        <main class="flex-grow container mx-auto px-4 py-6">
            <div class="bg-white rounded-lg shadow p-6 min-h-[calc(100vh-10rem)]">
                <RouterView />
            </div>
        </main>
    </div>
</template>
