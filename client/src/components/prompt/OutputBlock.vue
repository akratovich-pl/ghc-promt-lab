<template>
  <div 
    ref="blockRef"
    class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 transition-all duration-300 hover:shadow-xl"
  >
    <div class="flex items-center justify-between mb-4">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        💬 Output Response
      </h3>
      <div class="flex gap-2">
        <button
          v-if="outputText"
          @click="copyToClipboard"
          class="p-2 text-gray-600 dark:text-gray-400 hover:text-primary-600 
                 dark:hover:text-primary-400 transition-colors rounded-lg"
          :title="copied ? 'Copied!' : 'Copy to clipboard'"
        >
          {{ copied ? '✓' : '📋' }}
        </button>
      </div>
    </div>

    <div 
      v-if="isLoading"
      class="flex items-center justify-center h-40 text-gray-400"
    >
      <div class="text-center">
        <div ref="loadingRef" class="text-4xl mb-2">⏳</div>
        <div class="text-sm">Generating response...</div>
      </div>
    </div>

    <div 
      v-else-if="outputText"
      class="min-h-40 p-4 bg-gray-50 dark:bg-gray-700 rounded-lg 
             text-gray-900 dark:text-white whitespace-pre-wrap overflow-auto max-h-96"
    >
      {{ outputText }}
    </div>

    <div 
      v-else
      class="flex items-center justify-center h-40 text-gray-400 dark:text-gray-500"
    >
      <div class="text-center">
        <div class="text-4xl mb-2">📭</div>
        <div class="text-sm">No response yet</div>
      </div>
    </div>

    <!-- Metrics Panel -->
    <div v-if="showMetrics && outputText" class="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
      <MetricsDisplay
        :input-tokens="metrics.inputTokens"
        :output-tokens="metrics.outputTokens"
        :duration="metrics.duration"
        :cost="metrics.totalCost"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { gsap } from 'gsap'
import MetricsDisplay from './MetricsDisplay.vue'

export interface OutputBlockProps {
  outputText?: string
  isLoading?: boolean
  showMetrics?: boolean
  metrics?: {
    inputTokens: number
    outputTokens: number
    duration: number
    totalCost: number
  }
}

const props = withDefaults(defineProps<OutputBlockProps>(), {
  outputText: '',
  isLoading: false,
  showMetrics: true,
  metrics: () => ({
    inputTokens: 0,
    outputTokens: 0,
    duration: 0,
    totalCost: 0
  })
})

const blockRef = ref<HTMLDivElement>()
const loadingRef = ref<HTMLDivElement>()
const copied = ref(false)

async function copyToClipboard() {
  try {
    await navigator.clipboard.writeText(props.outputText)
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 2000)
  } catch (err) {
    console.error('Failed to copy:', err)
  }
}

watch(() => props.isLoading, (isLoading) => {
  if (isLoading && loadingRef.value) {
    gsap.to(loadingRef.value, {
      rotation: 360,
      duration: 2,
      repeat: -1,
      ease: 'linear'
    })
  }
})

watch(() => props.outputText, (newText, oldText) => {
  if (newText && !oldText && blockRef.value) {
    // Animate when output appears
    gsap.fromTo(blockRef.value,
      { scale: 0.98, opacity: 0.8 },
      { 
        scale: 1, 
        opacity: 1,
        duration: 0.4,
        ease: 'power2.out'
      }
    )
  }
})

onMounted(() => {
  // Entrance animation with more delay
  if (blockRef.value) {
    gsap.from(blockRef.value, {
      opacity: 0,
      y: 20,
      duration: 0.6,
      delay: 0.4,
      ease: 'power2.out'
    })
  }
})
</script>
