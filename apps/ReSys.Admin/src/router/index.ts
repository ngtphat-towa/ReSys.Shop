import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '../layout/AppLayout.vue'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          name: 'home',
          component: HomeView,
        },
        {
          path: 'about',
          name: 'about',
          component: () => import('../views/AboutView.vue'),
        },
        {
          path: 'Examples',
          name: 'Examples',
          component: () => import('../features/examples/views/ExampleList.vue'),
        },
        {
          path: 'Examples/create',
          name: 'Example-create',
          component: () => import('../features/examples/views/ExampleForm.vue'),
        },
        {
          path: 'Examples/edit/:id',
          name: 'Example-edit',
          component: () => import('../features/examples/views/ExampleForm.vue'),
        },
      ]
    }
  ],
})

export default router