<script setup lang="ts"></script>

<template>
    <div class="layout-footer bg-surface-0 dark:bg-surface-900 border-t border-surface-100 dark:border-surface-800 py-12">
        <div class="container mx-auto px-8 grid grid-cols-1 md:grid-cols-3 gap-12">
            <div>
                <div class="flex items-center gap-2 mb-6">
                    <svg width="30" height="35" viewBox="0 0 35 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M25.87 18.05L23.16 14.84L21.02 12.31L0.460022 36.63L2.61002 39.18L34.1 1.93L31.95 -0.620003L25.87 18.05Z" fill="var(--primary-color)" />
                        <path d="M19.14 26.03L16.43 22.82L14.29 20.29L3.58 33L5.73 35.55L19.14 19.66V26.03Z" fill="var(--primary-color)" />
                    </svg>
                    <span class="text-xl font-black text-surface-900 dark:text-surface-0">RESYS SHOP</span>
                </div>
                <p class="text-surface-500 dark:text-surface-400 leading-relaxed max-w-xs">
                    Providing high-quality premium products since 2025. Your satisfaction is our priority.
                </p>
            </div>
            <div>
                <h4 class="text-surface-900 dark:text-surface-0 font-bold mb-6 uppercase tracking-widest text-sm">Quick Links</h4>
                <ul class="list-none p-0 m-0 space-y-4">
                    <li><router-link to="/" class="text-surface-500 hover:text-primary transition-colors">Catalog</router-link></li>
                    <li><router-link to="/about" class="text-surface-500 hover:text-primary transition-colors">About Us</router-link></li>
                    <li><a href="#" class="text-surface-500 hover:text-primary transition-colors">Privacy Policy</a></li>
                </ul>
            </div>
            <div>
                <h4 class="text-surface-900 dark:text-surface-0 font-bold mb-6 uppercase tracking-widest text-sm">Connect</h4>
                <div class="flex gap-4">
                    <Button icon="pi pi-facebook" rounded text severity="secondary" />
                    <Button icon="pi pi-twitter" rounded text severity="secondary" />
                    <Button icon="pi pi-instagram" rounded text severity="secondary" />
                    <Button icon="pi pi-github" rounded text severity="secondary" />
                </div>
            </div>
        </div>
        <div class="text-center mt-12 pt-8 border-t border-surface-50 dark:border-surface-800">
            <p class="text-surface-400 text-sm font-medium">&copy; 2025 ReSys Shop. All rights reserved.</p>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.layout-footer {
    display: block;
}
</style>
