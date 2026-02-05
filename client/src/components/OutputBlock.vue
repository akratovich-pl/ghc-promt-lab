<template>
  <div 
    ref="containerRef"
    class="output-block bg-white rounded-lg shadow-md border border-gray-200 p-6"
  >
    <h2 class="text-2xl font-bold text-gray-900 mb-4">
      AI Response
    </h2>

    <!-- Loading State -->
    <div 
      v-if="promptStore.executing" 
      class="flex flex-col items-center justify-center py-12"
    >
      <div class="animate-spin rounded-full h-12 w-12 border-b-4 border-primary-600 mb-4"></div>
      <p class="text-gray-600">Generating response...</p>
      <p class="text-sm text-gray-500 mt-2">This may take a few seconds</p>
    </div>

    <!-- Response Content -->
    <div v-else-if="promptStore.response" class="space-y-4">
      <!-- Metrics Bar -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <div class="metric-card bg-gradient-to-br from-blue-50 to-blue-100 border border-blue-200 p-4 rounded-lg">
          <p class="text-xs text-blue-700 font-medium">Input Tokens</p>
          <p class="text-2xl font-bold text-blue-900">
            {{ promptStore.response.inputTokens.toLocaleString() }}
          </p>
        </div>
        
        <div class="metric-card bg-gradient-to-br from-green-50 to-green-100 border border-green-200 p-4 rounded-lg">
          <p class="text-xs text-green-700 font-medium">Output Tokens</p>
          <p class="text-2xl font-bold text-green-900">
            {{ promptStore.response.outputTokens.toLocaleString() }}
          </p>
        </div>
        
        <div class="metric-card bg-gradient-to-br from-purple-50 to-purple-100 border border-purple-200 p-4 rounded-lg">
          <p class="text-xs text-purple-700 font-medium">Cost</p>
          <p class="text-2xl font-bold text-purple-900">
            ${{ promptStore.response.cost.toFixed(6) }}
          </p>
        </div>
        
        <div class="metric-card bg-gradient-to-br from-orange-50 to-orange-100 border border-orange-200 p-4 rounded-lg">
          <p class="text-xs text-orange-700 font-medium">Latency</p>
          <p class="text-2xl font-bold text-orange-900">
            {{ promptStore.response.latencyMs }}ms
          </p>
        </div>
      </div>

      <!-- Model Info -->
      <div class="flex items-center gap-2 text-sm text-gray-600 mb-4">
        <span class="font-medium">Model:</span>
        <span class="px-2 py-1 bg-gray-100 border border-gray-200 rounded">
          {{ promptStore.response.model }}
        </span>
        <span class="ml-4 text-xs">
          {{ new Date(promptStore.response.createdAt).toLocaleString() }}
        </span>
      </div>

      <!-- Response Content -->
      <div class="response-content bg-gray-50 p-6 rounded-lg border border-gray-200">
        <div class="prose max-w-none">
          <pre class="whitespace-pre-wrap text-gray-900 font-sans text-base leading-relaxed">{{ promptStore.response.content }}</pre>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex gap-4 mt-4">
        <button
          @click="copyToClipboard"
          class="px-4 py-2 bg-blue-100 hover:bg-blue-200 border border-blue-200 hover:border-blue-300
                 text-blue-800 font-medium rounded-lg transition-all duration-200"
        >
          {{ copied ? '✓ Copied!' : 'Copy Response' }}
        </button>
      </div>
    </div>

    <!-- Empty State -->
    <div 
      v-else 
      class="flex flex-col items-center justify-center py-12 text-gray-500"
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
