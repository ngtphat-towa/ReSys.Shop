import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '../layout/app-layout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: AppLayout,
      meta: { breadcrumb: 'navigation.home' },
      children: [
        {
          path: '',
          name: 'shop.home',
          component: () => import('../features/examples/example-list.vue'),
        },
        {
          path: 'Examples/:id',
          name: 'shop.examples.detail',
          component: () => import('../features/examples/example-detail.vue'),
          meta: { breadcrumb: 'titles.detail' }
        },
        {
          path: 'about',
          name: 'shop.about',
          component: () => import('../views/about-view.vue'),
          meta: { breadcrumb: 'navigation.about' }
        },
      ]
    }
  ],
})

export default router