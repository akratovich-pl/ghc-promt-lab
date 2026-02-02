import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue')
    },
    {
      path: '/lab',
      name: 'prompt-lab',
      component: () => import('@/views/PromptLabView.vue')
    }
  ]
})

export default router
