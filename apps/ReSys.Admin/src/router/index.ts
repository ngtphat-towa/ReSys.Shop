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
          path: 'products',
          name: 'products',
          component: () => import('../views/products/ProductList.vue'),
        },
        {
          path: 'products/create',
          name: 'product-create',
          component: () => import('../views/products/ProductForm.vue'),
        },
        {
          path: 'products/edit/:id',
          name: 'product-edit',
          component: () => import('../views/products/ProductForm.vue'),
        },
      ]
    }
  ],
})

export default router