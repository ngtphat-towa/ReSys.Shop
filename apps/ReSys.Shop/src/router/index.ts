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
          path: 'products/:id',
          name: 'product-detail',
          component: () => import('../views/ProductDetailView.vue'),
        },
        {
          path: 'about',
          name: 'about',
          component: () => import('../views/AboutView.vue'),
        },
      ]
    }
  ],
})

export default router
