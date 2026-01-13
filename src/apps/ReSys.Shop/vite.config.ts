import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'
import Components from 'unplugin-vue-components/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    tailwindcss(),
    Components({
      resolvers: [PrimeVueResolver()],
      dts: 'src/shared/components.d.ts'
    })
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  base: '/',
  server: {
    host: true,
    port: parseInt(process.env.PORT ?? '5174'),
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})