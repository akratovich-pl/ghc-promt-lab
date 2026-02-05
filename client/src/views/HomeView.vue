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
            :class="apiConnected ? 'bg-emerald-100 border-emerald-200' : 'bg-red-100 border-red-200'"
          >
            <span :class="apiConnected ? 'text-emerald-800' : 'text-red-800'">
              {{ apiConnected ? '✓ API Connected' : '✗ API Disconnected' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { gsap } from 'gsap'
import ModelSelection from '@/components/ModelSelection.vue'
import InputBlock from '@/components/InputBlock.vue'
import OutputBlock from '@/components/OutputBlock.vue'
import { useProviderStore } from '@/stores/providerStore'

const logoRef = ref<HTMLDivElement>()
const providerStore = useProviderStore()

const apiConnected = computed(() => {
  return providerStore.providers.length > 0
})

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
</script>

<style scoped>
/* Additional custom styles if needed */
</style>
