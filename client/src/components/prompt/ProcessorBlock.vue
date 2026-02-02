<template>
  <div 
    ref="blockRef"
    class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 transition-all duration-300"
    :class="stateClass"
  >
    <div class="flex items-center justify-between mb-4">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        {{ stateIcon }} Processor
      </h3>
      <div class="text-sm font-medium" :class="stateTextClass">
        {{ stateLabel }}
      </div>
    </div>

    <div class="space-y-4">
      <!-- LLM Info -->
      <div class="flex items-center gap-3 p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
        <div class="text-2xl">🤖</div>
        <div class="flex-1">
          <div class="text-sm font-medium text-gray-900 dark:text-white">
            {{ llmName || 'No Model Selected' }}
          </div>
          <div class="text-xs text-gray-600 dark:text-gray-400">
            {{ llmModel || 'Select a model to continue' }}
          </div>
        </div>
      </div>

      <!-- Processing Animation -->
      <div v-if="state === 'processing'" class="flex items-center justify-center py-4">
        <div class="flex gap-2">
          <div 
            v-for="i in 3" 
            :key="i"
            ref="dotRefs"
            class="w-3 h-3 bg-primary-500 rounded-full"
          />
        </div>
      </div>

      <!-- Token Calculation Display -->
      <div v-if="state !== 'idle'" class="grid grid-cols-2 gap-3 text-sm">
        <div class="p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
          <div class="text-xs text-blue-600 dark:text-blue-400 mb-1">Input</div>
          <div class="font-semibold text-blue-900 dark:text-blue-200">
            {{ inputTokens }} tokens
          </div>
        </div>
        <div class="p-3 bg-green-50 dark:bg-green-900/30 rounded-lg">
          <div class="text-xs text-green-600 dark:text-green-400 mb-1">Output</div>
          <div class="font-semibold text-green-900 dark:text-green-200">
            {{ outputTokens }} tokens
          </div>
        </div>
      </div>

      <!-- Error Display -->
      <div v-if="state === 'error'" class="p-3 bg-red-50 dark:bg-red-900/30 rounded-lg">
        <div class="text-sm text-red-800 dark:text-red-200">
          {{ errorMessage || 'An error occurred during processing' }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import { gsap } from 'gsap'

export interface ProcessorBlockProps {
  state?: 'idle' | 'processing' | 'completed' | 'error'
  llmName?: string
  llmModel?: string
  inputTokens?: number
  outputTokens?: number
  errorMessage?: string
}

const props = withDefaults(defineProps<ProcessorBlockProps>(), {
  state: 'idle',
  llmName: '',
  llmModel: '',
  inputTokens: 0,
  outputTokens: 0,
  errorMessage: ''
})

const blockRef = ref<HTMLDivElement>()
const dotRefs = ref<HTMLDivElement[]>([])

const stateClass = computed(() => {
  switch (props.state) {
    case 'processing':
      return 'ring-2 ring-primary-500 shadow-xl'
    case 'completed':
      return 'ring-2 ring-green-500'
    case 'error':
      return 'ring-2 ring-red-500'
    default:
      return ''
  }
})

const stateTextClass = computed(() => {
  switch (props.state) {
    case 'processing':
      return 'text-primary-600 dark:text-primary-400'
    case 'completed':
      return 'text-green-600 dark:text-green-400'
    case 'error':
      return 'text-red-600 dark:text-red-400'
    default:
      return 'text-gray-600 dark:text-gray-400'
  }
})

const stateIcon = computed(() => {
  switch (props.state) {
    case 'processing':
      return '⚡'
    case 'completed':
      return '✅'
    case 'error':
      return '❌'
    default:
      return '💭'
  }
})

const stateLabel = computed(() => {
  switch (props.state) {
    case 'processing':
      return 'Processing...'
    case 'completed':
      return 'Completed'
    case 'error':
      return 'Error'
    default:
      return 'Ready'
  }
})

watch(() => props.state, async (newState) => {
  if (newState === 'processing') {
    await nextTick()
    animateProcessing()
  } else if (newState === 'completed') {
    animateSuccess()
  } else if (newState === 'error') {
    animateError()
  }
})

function animateProcessing() {
  if (dotRefs.value.length > 0) {
    gsap.to(dotRefs.value, {
      y: -10,
      duration: 0.4,
      stagger: 0.1,
      repeat: -1,
      yoyo: true,
      ease: 'power1.inOut'
    })
  }
}

function animateSuccess() {
  if (blockRef.value) {
    gsap.fromTo(blockRef.value, 
      { scale: 1 },
      { 
        scale: 1.02, 
        duration: 0.3, 
        yoyo: true, 
        repeat: 1,
        ease: 'power2.inOut'
      }
    )
  }
}

function animateError() {
  if (blockRef.value) {
    gsap.fromTo(blockRef.value,
      { x: 0 },
      { 
        x: -10, 
        duration: 0.1, 
        yoyo: true, 
        repeat: 3,
        ease: 'power2.inOut'
      }
    )
  }
}

onMounted(() => {
  // Entrance animation with delay
  if (blockRef.value) {
    gsap.from(blockRef.value, {
      opacity: 0,
      y: 20,
      duration: 0.6,
      delay: 0.2,
      ease: 'power2.out'
    })
  }
})
</script>
