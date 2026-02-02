<template>
  <div 
    ref="containerRef"
    class="input-block bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6"
  >
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">
      Input Prompt
    </h2>

    <!-- System Prompt (Optional) -->
    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        System Prompt (Optional)
      </label>
      <textarea
        v-model="localSystemPrompt"
        placeholder="Provide context or instructions for the AI..."
        class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg 
               bg-white dark:bg-gray-700 text-gray-900 dark:text-white
               focus:ring-2 focus:ring-primary-500 focus:border-transparent
               resize-none"
        rows="2"
      ></textarea>
    </div>

    <!-- Main Prompt Input -->
    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Your Prompt <span class="text-red-500">*</span>
      </label>
      <textarea
        v-model="localPrompt"
        placeholder="Enter your prompt here..."
        class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg 
               bg-white dark:bg-gray-700 text-gray-900 dark:text-white
               focus:ring-2 focus:ring-primary-500 focus:border-transparent
               resize-none"
        rows="6"
      ></textarea>
    </div>

    <!-- Token Estimation -->
    <div 
      v-if="promptStore.tokenEstimate" 
      class="mb-4 p-4 bg-blue-50 dark:bg-blue-900/30 border border-blue-200 dark:border-blue-800 rounded-lg"
    >
      <div class="flex justify-between items-center">
        <div>
          <p class="text-sm font-medium text-blue-900 dark:text-blue-300">
            Estimated Tokens: 
            <span class="font-bold">{{ promptStore.tokenEstimate.tokenCount }}</span>
          </p>
          <p class="text-xs text-blue-700 dark:text-blue-400 mt-1">
            Estimated Cost: ${{ promptStore.tokenEstimate.estimatedCost.toFixed(6) }}
          </p>
        </div>
        <div 
          v-if="promptStore.estimating" 
          class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"
        ></div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex gap-4">
      <button
        @click="handleExecute"
        :disabled="!canExecute || promptStore.executing"
        class="flex-1 px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold 
               rounded-lg shadow-lg transition-all duration-200 disabled:opacity-50 
               disabled:cursor-not-allowed flex items-center justify-center gap-2"
      >
        <span v-if="promptStore.executing" class="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></span>
        <span>{{ promptStore.executing ? 'Executing...' : 'Execute Prompt' }}</span>
      </button>
      
      <button
        @click="handleClear"
        :disabled="promptStore.executing"
        class="px-6 py-3 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 
               text-gray-900 dark:text-white font-semibold rounded-lg shadow-lg 
               transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        Clear
      </button>
    </div>

    <!-- Error Display -->
    <div 
      v-if="promptStore.error" 
      class="mt-4 p-4 bg-red-100 dark:bg-red-900/30 border border-red-400 text-red-700 dark:text-red-300 rounded"
    >
      {{ promptStore.error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useDebounceFn } from '@vueuse/core'
import { gsap } from 'gsap'
import { usePromptStore } from '@/stores/promptStore'
import { useProviderStore } from '@/stores/providerStore'

const promptStore = usePromptStore()
const providerStore = useProviderStore()

const containerRef = ref<HTMLDivElement>()
const localPrompt = ref('')
const localSystemPrompt = ref('')

const canExecute = computed(() => {
  return localPrompt.value.trim().length > 0 && 
         providerStore.selectedModel.length > 0 &&
         !promptStore.executing
})

// Debounced token estimation (500ms)
const debouncedEstimateTokens = useDebounceFn(async (prompt: string) => {
  if (prompt.trim().length > 0 && providerStore.selectedModel) {
    await promptStore.estimateTokens(prompt, providerStore.selectedModel)
  } else {
    promptStore.clearTokenEstimate()
  }
}, 500)

// Watch for prompt changes and trigger debounced estimation
watch(localPrompt, (newValue) => {
  debouncedEstimateTokens(newValue)
})

// Watch for model changes and re-estimate if we have a prompt
watch(() => providerStore.selectedModel, () => {
  if (localPrompt.value.trim().length > 0) {
    debouncedEstimateTokens(localPrompt.value)
  }
})

const handleExecute = async () => {
  if (!canExecute.value) return

  try {
    // Animate on execution start
    if (containerRef.value) {
      gsap.to(containerRef.value, {
        scale: 0.98,
        duration: 0.2,
        ease: 'power2.out',
        yoyo: true,
        repeat: 1
      })
    }

    await promptStore.executePrompt(
      localPrompt.value,
      providerStore.selectedModel,
      localSystemPrompt.value || undefined
    )
  } catch (error) {
    console.error('Error executing prompt:', error)
  }
}

const handleClear = () => {
  localPrompt.value = ''
  localSystemPrompt.value = ''
  promptStore.clearResponse()
  promptStore.clearTokenEstimate()
}
</script>

<style scoped>
/* Additional styles if needed */
</style>
