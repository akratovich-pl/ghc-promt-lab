<template>
  <div class="flex flex-col items-center justify-center min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
    <div 
      ref="logoRef"
      class="mb-8 text-8xl animate-pulse-slow"
    >
      🚀
    </div>
    <h1 class="text-5xl font-bold text-gray-900 dark:text-white mb-4">
      PromptLab
    </h1>
    <p class="text-xl text-gray-600 dark:text-gray-300 mb-8">
      AI Prompt Testing & Visualization Tool
    </p>
    
    <div class="flex gap-4 mb-8">
      <div class="px-4 py-2 bg-green-100 dark:bg-green-900 rounded-lg">
        <span class="text-green-800 dark:text-green-200">✓ Vue 3</span>
      </div>
      <div class="px-4 py-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
        <span class="text-blue-800 dark:text-blue-200">✓ Tailwind CSS</span>
      </div>
      <div class="px-4 py-2 bg-purple-100 dark:bg-purple-900 rounded-lg">
        <span class="text-purple-800 dark:text-purple-200">✓ GSAP</span>
      </div>
      <div class="px-4 py-2 bg-pink-100 dark:bg-pink-900 rounded-lg">
        <span class="text-pink-800 dark:text-pink-200">✓ VueUse</span>
      </div>
    </div>

    <button
      @click="animateRocket"
      class="px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-lg shadow-lg transition-colors duration-200"
    >
      Test Animation (GSAP)
    </button>

    <div class="mt-8 text-gray-500 dark:text-gray-400">
      <p>Mouse position: {{ x }}, {{ y }}</p>
      <p class="text-sm">(VueUse demo)</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useMouse } from '@vueuse/core'
import { gsap } from 'gsap'

// VueUse composable
const { x, y } = useMouse()

// GSAP animation ref
const logoRef = ref<HTMLDivElement>()

const animateRocket = () => {
  if (logoRef.value) {
    gsap.to(logoRef.value, {
      y: -50,
      rotation: 360,
      duration: 1,
      ease: 'elastic.out(1, 0.3)',
      onComplete: () => {
        gsap.to(logoRef.value, {
          y: 0,
          rotation: 0,
          duration: 0.5,
          ease: 'bounce.out'
        })
      }
    })
  }
}
</script>

<style scoped>
/* Additional custom styles if needed */
</style>
