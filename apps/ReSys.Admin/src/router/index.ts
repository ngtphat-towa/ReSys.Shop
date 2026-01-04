import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '../layout/app-layout.vue'
import HomeView from '../views/home-view.vue'

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
          name: 'home',
          component: HomeView,
        },
        {
          path: 'about',
          name: 'about',
          component: () => import('../views/about-view.vue'),
          meta: { breadcrumb: 'navigation.about' }
        },
        {
          path: 'testing',
          meta: { breadcrumb: 'navigation.testing' },
          children: [
            {
              path: 'examples',
              meta: { breadcrumb: 'titles.breadcrumb_parent' },
              children: [
                {
                  path: '',
                  name: 'testing.examples.list',
                  component: () => import('../features/testing/examples/views/example-list.vue'),
                },
                {
                  path: 'create',
                  name: 'testing.examples.create',
                  component: () => import('../features/testing/examples/views/example-form.vue'),
                  meta: { breadcrumb: 'actions.create' }
                },
                {
                  path: 'edit/:id',
                  name: 'testing.examples.edit',
                  component: () => import('../features/testing/examples/views/example-form.vue'),
                  meta: { breadcrumb: 'actions.edit' }
                }
              ]
            }
          ]
        }
      ]
    }
  ],
})

export default router