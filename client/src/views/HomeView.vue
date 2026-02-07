<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-white to-gray-50 py-8 px-4">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="text-center mb-8">
        <div 
          ref="logoRef"
          class="inline-block text-6xl mb-4 cursor-pointer"
          @click="animateLogo"
        >
          🚀
        </div>
        <h1 class="text-4xl md:text-5xl font-bold text-gray-900 mb-2">
          PromptLab
        </h1>
        <p class="text-lg text-gray-600">
          AI Prompt Testing & Visualization Tool
        </p>
      </div>

      <!-- API Connection Status Banner -->
      <div 
        v-if="!llmStore.isApiConnected"
        class="mb-6 bg-red-50 border-l-4 border-red-500 p-4 rounded-lg shadow-md max-w-3xl mx-auto"
      >
        <div class="flex items-start">
          <div class="flex-shrink-0">
            <svg class="w-6 h-6 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          <div class="ml-3 flex-1">
            <h3 class="text-sm font-semibold text-red-800">
              Backend API Unavailable
            </h3>
            <p class="mt-1 text-sm text-red-700">
              The backend service is currently unavailable. Please ensure the API server is running to use PromptLab features.
            </p>
          </div>
        </div>
      </div>

    <div class="flex gap-4">
      <button
        @click="animateRocket"
        class="px-6 py-3 bg-blue-500 hover:bg-blue-600 text-white font-semibold rounded-lg shadow-md transition-all duration-200"
      >
        Test Animation (GSAP)
      </button>
      
      <router-link
        to="/lab"
        class="px-6 py-3 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg shadow-md transition-all duration-200"
      >
        🧪 Open Prompt Lab
      </router-link>
    </div>

    <!-- Input and Output in 2-column layout on larger screens -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <InputBlock />
      <OutputBlock />
    </div>

    <!-- Footer with status indicators -->
    <div class="mt-8 text-center">
        <div class="flex gap-3 justify-center flex-wrap">
          <div class="px-3 py-1.5 bg-green-100 border border-green-200 rounded-lg text-sm">
            <span class="text-green-800">✓ Vue 3</span>
          </div>
          <div class="px-3 py-1.5 bg-blue-100 border border-blue-200 rounded-lg text-sm">
            <span class="text-blue-800">✓ Pinia</span>
          </div>
          <div class="px-3 py-1.5 bg-purple-100 border border-purple-200 rounded-lg text-sm">
            <span class="text-purple-800">✓ GSAP</span>
          </div>
          <div class="px-3 py-1.5 bg-pink-100 border border-pink-200 rounded-lg text-sm">
            <span class="text-pink-800">✓ VueUse</span>
          </div>
          <div 
            class="px-3 py-1.5 rounded-lg text-sm border"
            :class="llmStore.isApiConnected ? 'bg-emerald-100 border-emerald-200' : 'bg-red-100 border-red-200'"
          >
            <span :class="llmStore.isApiConnected ? 'text-emerald-800' : 'text-red-800'">
              {{ llmStore.isApiConnected ? '✓ API Connected' : '✗ API Disconnected' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { gsap } from 'gsap'
import ModelSelection from '@/components/ModelSelection.vue'
import InputBlock from '@/components/InputBlock.vue'
import OutputBlock from '@/components/OutputBlock.vue'
import { useLlmStore } from '@/stores/llmStore'

const logoRef = ref<HTMLDivElement>()
const llmStore = useLlmStore()

const animateLogo = () => {
  if (logoRef.value) {
    gsap.to(logoRef.value, {
      y: -30,
      rotation: 360,
      scale: 1.2,
      duration: 0.8,
      ease: 'elastic.out(1, 0.3)',
      onComplete: () => {
        gsap.to(logoRef.value, {
          y: 0,
          rotation: 0,
          scale: 1,
          duration: 0.5,
          ease: 'bounce.out'
        })
      }
    })
  }
}

onMounted(() => {
  // Start monitoring API connection
  llmStore.startConnectionMonitoring()
  
  // Entrance animation
  gsap.fromTo(
    '.grid > *',
    { opacity: 0, y: 30 },
    { 
      opacity: 1, 
      y: 0, 
      duration: 0.6,
      stagger: 0.15,
      ease: 'power2.out'
    }
  )
})

onUnmounted(() => {
  // Stop monitoring when component is destroyed
  llmStore.stopConnectionMonitoring()
})
</script>

<style scoped>
/* Additional custom styles if needed */
</style>
