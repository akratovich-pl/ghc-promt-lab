<template>
  <div 
    ref="containerRef"
    class="output-block bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
  >
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">
      AI Response
    </h2>

    <!-- Loading State -->
    <div 
      v-if="promptStore.executing" 
      class="flex flex-col items-center justify-center py-12"
    >
      <div class="animate-spin rounded-full h-12 w-12 border-b-4 border-primary-600 mb-4"></div>
      <p class="text-gray-600 dark:text-gray-400">Generating response...</p>
      <p class="text-sm text-gray-500 dark:text-gray-500 mt-2">This may take a few seconds</p>
    </div>

    <!-- Response Content -->
    <div v-else-if="promptStore.response" class="space-y-4">
      <!-- Metrics Bar -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <div class="metric-card bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900/30 dark:to-blue-800/30 p-4 rounded-lg">
          <p class="text-xs text-blue-600 dark:text-blue-400 font-medium">Input Tokens</p>
          <p class="text-2xl font-bold text-blue-900 dark:text-blue-300">
            {{ promptStore.response.inputTokens.toLocaleString() }}
          </p>
        </div>
        
        <div class="metric-card bg-gradient-to-br from-green-50 to-green-100 dark:from-green-900/30 dark:to-green-800/30 p-4 rounded-lg">
          <p class="text-xs text-green-600 dark:text-green-400 font-medium">Output Tokens</p>
          <p class="text-2xl font-bold text-green-900 dark:text-green-300">
            {{ promptStore.response.outputTokens.toLocaleString() }}
          </p>
        </div>
        
        <div class="metric-card bg-gradient-to-br from-purple-50 to-purple-100 dark:from-purple-900/30 dark:to-purple-800/30 p-4 rounded-lg">
          <p class="text-xs text-purple-600 dark:text-purple-400 font-medium">Cost</p>
          <p class="text-2xl font-bold text-purple-900 dark:text-purple-300">
            ${{ promptStore.response.cost.toFixed(6) }}
          </p>
        </div>
        
        <div class="metric-card bg-gradient-to-br from-orange-50 to-orange-100 dark:from-orange-900/30 dark:to-orange-800/30 p-4 rounded-lg">
          <p class="text-xs text-orange-600 dark:text-orange-400 font-medium">Latency</p>
          <p class="text-2xl font-bold text-orange-900 dark:text-orange-300">
            {{ promptStore.response.latencyMs }}ms
          </p>
        </div>
      </div>

      <!-- Model Info -->
      <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 mb-4">
        <span class="font-medium">Model:</span>
        <span class="px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded">
          {{ promptStore.response.model }}
        </span>
        <span class="ml-4 text-xs">
          {{ new Date(promptStore.response.createdAt).toLocaleString() }}
        </span>
      </div>

      <!-- Response Content -->
      <div class="response-content bg-gray-50 dark:bg-gray-900 p-6 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="prose dark:prose-invert max-w-none">
          <pre class="whitespace-pre-wrap text-gray-900 dark:text-gray-100 font-sans text-base leading-relaxed">{{ promptStore.response.content }}</pre>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex gap-4 mt-4">
        <button
          @click="copyToClipboard"
          class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 
                 text-gray-900 dark:text-white font-medium rounded-lg transition-colors duration-200"
        >
          {{ copied ? '✓ Copied!' : 'Copy Response' }}
        </button>
      </div>
    </div>

    <!-- Empty State -->
    <div 
      v-else 
      class="flex flex-col items-center justify-center py-12 text-gray-400 dark:text-gray-600"
    >
      <div class="text-6xl mb-4">💬</div>
      <p class="text-lg font-medium">No response yet</p>
      <p class="text-sm mt-2">Execute a prompt to see the AI response here</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { gsap } from 'gsap'
import { usePromptStore } from '@/stores/promptStore'

const promptStore = usePromptStore()
const containerRef = ref<HTMLDivElement>()
const copied = ref(false)

// Animate when response arrives
watch(() => promptStore.response, (newResponse, oldResponse) => {
  if (newResponse && !oldResponse && containerRef.value) {
    // Entrance animation
    gsap.fromTo(
      containerRef.value,
      { 
        opacity: 0.5, 
        y: 20,
        scale: 0.95
      },
      { 
        opacity: 1, 
        y: 0,
        scale: 1,
        duration: 0.6,
        ease: 'back.out(1.4)'
      }
    )
    
    // Animate metrics cards
    const metricCards = containerRef.value.querySelectorAll('.metric-card')
    gsap.fromTo(
      metricCards,
      { opacity: 0, y: 20 },
      { 
        opacity: 1, 
        y: 0, 
        duration: 0.4,
        stagger: 0.1,
        delay: 0.2,
        ease: 'power2.out'
      }
    )
  }
})

const copyToClipboard = async () => {
  if (promptStore.response?.content) {
    try {
      await navigator.clipboard.writeText(promptStore.response.content)
      copied.value = true
      setTimeout(() => {
        copied.value = false
      }, 2000)
    } catch (err) {
      console.error('Failed to copy:', err)
    }
  }
}
</script>

<style scoped>
.response-content {
  max-height: 600px;
  overflow-y: auto;
}

.response-content::-webkit-scrollbar {
  width: 8px;
}

.response-content::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.1);
  border-radius: 4px;
}

.response-content::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 4px;
}

.response-content::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.5);
}
</style>
