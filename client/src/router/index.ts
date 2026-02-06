import { createRouter, createWebHistory } from 'vue-router'
import { useLlmStore } from '@/stores/llmStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/model-selection'
    },
    {
      path: '/model-selection',
      name: 'model-selection',
      component: () => import('@/views/ModelSelectionView.vue')
    },
    {
      path: '/prompt-lab',
      name: 'prompt-lab',
      component: () => import('@/views/PromptLabView.vue'),
      meta: { requiresModel: true }
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('@/views/AboutView.vue')
    },
    {
      path: '/demo',
      name: 'demo',
      component: () => import('@/views/HomeView.vue')
    }
  ]
})

// Navigation guard to require model selection
router.beforeEach((to, _from, next) => {
  // Check if route requires model selection
  if (to.meta.requiresModel) {
    const llmStore = useLlmStore()
    
    if (!llmStore.hasSelectedModel) {
      // Redirect to model selection if no model is selected
      next({ name: 'model-selection' })
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router
