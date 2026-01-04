<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useExamplestore } from '@/stores/Examplestore';
import { useRouter } from 'vue-router';
import { useConfirm } from "primevue/useconfirm";
import { storeToRefs } from 'pinia';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import IconField from 'primevue/iconfield';
import InputIcon from 'primevue/inputicon';
import Tag from 'primevue/tag';

const Examplestore = useExamplestore();
const { Examples, loading, totalRecords, pagination } = storeToRefs(Examplestore);
const router = useRouter();
const confirm = useConfirm();
const search = ref('');

const loadExamples = async () => {
    await Examplestore.fetchExamples({ search: search.value });
};

const onPage = (event: any) => {
    pagination.value.page = event.page + 1;
    pagination.value.pageSize = event.rows;
    loadExamples();
};

const onSearch = () => {
    pagination.value.page = 1;
    loadExamples();
};

const editExample = (id: string) => {
    router.push(`/Examples/edit/${id}`);
};

const confirmDelete = (Example: any) => {
    confirm.require({
        message: `Are you sure you want to delete "${Example.name}"? This action cannot be undone.`,
        header: 'Dangerous Action',
        icon: 'pi pi-info-circle',
        rejectLabel: 'Cancel',
        rejectProps: {
            label: 'Cancel',
            severity: 'secondary',
            outlined: true
        },
        acceptProps: {
            label: 'Delete',
            severity: 'danger'
        },
        accept: async () => {
            await Examplestore.deleteExample(Example.id);
        }
    });
};

onMounted(() => {
    loadExamples();
});
</script>

<template>
    <div class="p-6">
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
            <div>
                <h1 class="text-3xl font-black text-gray-900 tracking-tight">Example Catalog</h1>
                <p class="text-gray-500 mt-1">Manage your store inventory and Example details.</p>
            </div>
            <div class="flex gap-3 w-full md:w-auto">
                <IconField iconPosition="left" class="w-full md:w-72">
                    <InputIcon class="pi pi-search" />
                    <InputText v-model="search" placeholder="Quick search..." class="w-full" @keyup.enter="onSearch" />
                </IconField>
                <Button label="Add Example" icon="pi pi-plus" @click="router.push('/Examples/create')" class="shadow-md" />
            </div>
        </div>

        <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <DataTable 
                :value="Examples" 
                :loading="loading" 
                lazy 
                paginator 
                :rows="pagination.pageSize" 
                :totalRecords="totalRecords" 
                :first="(pagination.page - 1) * pagination.pageSize"
                @page="onPage"
                tableStyle="min-width: 60rem"
                class="p-datatable-sm"
            >
                <template #empty>
                    <div class="flex flex-col items-center justify-center py-20 text-gray-400">
                        <i class="pi pi-box text-6xl mb-4 opacity-20"></i>
                        <p class="text-xl font-medium">No Examples found</p>
                    </div>
                </template>

                <Column field="image_url" header="Preview" class="w-24">
                    <template #body="slotProps">
                        <div class="relative w-16 h-16 rounded-xl overflow-hidden bg-gray-50 border border-gray-100 shadow-sm group">
                            <img v-if="slotProps.data.image_url" :src="slotProps.data.image_url" :alt="slotProps.data.name" class="w-full h-full object-cover" />
                            <div v-else class="w-full h-full flex items-center justify-center text-gray-300">
                                <i class="pi pi-image text-xl"></i>
                            </div>
                        </div>
                    </template>
                </Column>

                <Column field="name" header="Example Name" sortable class="font-bold text-gray-800"></Column>
                
                <Column field="description" header="Description" class="max-w-xs">
                    <template #body="slotProps">
                        <p class="text-sm text-gray-500 line-clamp-2 leading-relaxed">
                            {{ slotProps.data.description || 'No description provided' }}
                        </p>
                    </template>
                </Column>

                <Column field="price" header="Price" sortable>
                    <template #body="slotProps">
                        <span class="font-black text-gray-900">
                            {{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(slotProps.data.price) }}
                        </span>
                    </template>
                </Column>

                <Column header="Status">
                    <template #body>
                        <Tag severity="success" value="Active" rounded />
                    </template>
                </Column>

                <Column header="Actions" class="w-32 text-right" headerClass="text-right">
                    <template #body="slotProps">
                        <div class="flex justify-end gap-1">
                            <Button icon="pi pi-pencil" severity="info" text rounded @click="editExample(slotProps.data.id)" v-tooltip.top="'Edit Example'" />
                            <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(slotProps.data)" v-tooltip.top="'Delete Example'" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
</template>

<style scoped>
:deep(.p-datatable-header) {
    background: transparent;
    padding: 0;
}
:deep(.p-datatable-thead > tr > th) {
    background: #f9fafb;
    color: #4b5563;
    font-size: 0.875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.025em;
    padding: 1rem 1.5rem;
}
:deep(.p-datatable-tbody > tr > td) {
    padding: 1rem 1.5rem;
    border-bottom: 1px solid #f3f4f6;
}
:deep(.p-datatable-tbody > tr:hover) {
    background: #fdfdfd;
}
</style>
