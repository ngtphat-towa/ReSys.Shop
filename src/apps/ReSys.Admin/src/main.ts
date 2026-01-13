import '@/assets/scss/main.scss';
import '@/assets/tailwind.css';

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './app.vue'
import router from './router'

import PrimeVue from 'primevue/config';
import Aura from '@primevue/themes/aura';
import ToastService from 'primevue/toastservice';
import ConfirmationService from 'primevue/confirmationservice';
import StyleClass from 'primevue/styleclass';
import Tooltip from 'primevue/tooltip';

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
    theme: {
        preset: Aura,
        options: {
            darkModeSelector: '.app-dark'
        }
    }
});
app.use(ToastService);
app.use(ConfirmationService);
app.directive('styleclass', StyleClass);
app.directive('tooltip', Tooltip);

app.mount('#app')