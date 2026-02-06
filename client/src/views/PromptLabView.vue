<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-white to-gray-50">
    <!-- Header -->
    <header class="bg-white border-b border-gray-200 shadow-sm">
      <div class="max-w-7xl mx-auto px-4 py-5 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <img 
              src="/logo.png" 
              alt="PromptLab Logo" 
              class="w-12 h-12 rounded-lg"
            />
            <div>
              <h1 class="text-3xl font-bold text-gray-900">
                PromptLab
              </h1>
              <p class="text-base text-gray-500 italic">
                Where AI Prompts Come to Life
              </p>
            </div>
          </div>
          <div class="flex items-center gap-4">
            <p class="text-sm text-gray-600 font-medium">
              {{ llmStore.selectedModel?.providerName }} - {{ llmStore.selectedModel?.modelName }}
            </p>
            <button
              @click="changeModel"
              class="px-5 py-2.5 bg-blue-100 hover:bg-blue-200 text-blue-800 font-medium rounded-lg transition-colors"
            >
              Change Model
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Left Column: Prompt Input -->
        <div class="lg:col-span-2 space-y-6">
          <!-- Prompt Input Card -->
          <div 
            ref="promptCard"
            class="bg-white rounded-xl shadow-md border border-gray-200 p-6"
          >
            <h2 class="text-xl font-semibold text-gray-900 mb-4">
              Prompt Input
            </h2>
            <textarea
              v-model="promptStore.currentPrompt"
              placeholder="Enter your prompt here..."
              rows="8"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-200 focus:border-blue-400 focus:outline-none bg-white text-gray-900 placeholder-gray-400 resize-none transition-colors"
            ></textarea>
            <div class="mt-4 flex items-center justify-between">
              <div class="text-sm text-gray-600 font-medium">
                Characters: {{ promptStore.currentPrompt.length }}
              </div>
              <button
                @click="executePrompt"
                :disabled="!promptStore.currentPrompt.trim() || promptStore.isExecuting"
                class="px-6 py-3 bg-blue-100 hover:bg-blue-200 disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed text-blue-800 font-semibold rounded-lg transition-colors"
              >
                {{ promptStore.isExecuting ? 'Executing...' : 'Execute' }}
              </button>
            </div>
          </div>

          <!-- Response Card -->
          <div 
            ref="responseCard"
            class="bg-white rounded-xl shadow-md border border-gray-200 p-6"
          >
            <h2 class="text-xl font-semibold text-gray-900 mb-4">
              Response
            </h2>
            <div 
              v-if="promptStore.currentResponse"
              class="prose max-w-none p-4 bg-gray-50 rounded-lg min-h-[200px] text-gray-900 border border-gray-200"
            >
              {{ promptStore.currentResponse }}
            </div>
            <div 
              v-else
              class="text-gray-500 italic p-4 bg-gray-50 rounded-lg min-h-[200px] flex items-center justify-center border border-gray-200"
            >
              Response will appear here after execution
            </div>
          </div>
        </div>

        <!-- Right Column: Metrics & History -->
        <div class="space-y-6">
          <!-- Metrics Card -->
          <div 
            ref="metricsCard"
            class="bg-white rounded-xl shadow-md border border-gray-200 p-6"
          >
            <h2 class="text-xl font-semibold text-gray-900 mb-4">
              Metrics
            </h2>
            <div class="space-y-4">
              <div class="flex justify-between items-center py-2 border-b border-gray-100">
                <span class="text-gray-700 font-medium">Total Executions</span>
                <span class="font-bold text-gray-900 text-lg">
                  {{ metricsStore.totalExecutions }}
                </span>
              </div>
              <div class="flex justify-between items-center py-2 border-b border-gray-100">
                <span class="text-gray-700 font-medium">Total Tokens</span>
                <span class="font-bold text-gray-900 text-lg">
                  {{ metricsStore.totalTokensUsed.toLocaleString() }}
                </span>
              </div>
              <div class="flex justify-between items-center py-2 border-b border-gray-100">
                <span class="text-gray-700 font-medium">Total Cost</span>
                <span class="font-bold text-accent-green text-lg">
                  ${{ metricsStore.totalCost.toFixed(4) }}
                </span>
              </div>
              <div class="flex justify-between items-center py-2">
                <span class="text-gray-700 font-medium">Avg Time</span>
                <span class="font-bold text-gray-900 text-lg">
                  {{ (metricsStore.averageExecutionTime / 1000).toFixed(2) }}s
                </span>
              </div>
            </div>
          </div>

          <!-- History Card -->
          <div class="bg-white rounded-xl shadow-md border border-gray-200 p-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-xl font-semibold text-gray-900">
                History
              </h2>
              <button
                v-if="promptStore.hasHistory"
                @click="promptStore.clearHistory"
                class="text-sm px-4 py-2 bg-blue-100 hover:bg-blue-200 text-blue-800 font-medium rounded-lg transition-colors"
              >
                Clear
              </button>
            </div>
            <div v-if="promptStore.hasHistory" class="space-y-2 max-h-[400px] overflow-y-auto">
              <div
                v-for="execution in promptStore.executionHistory.slice(0, 10)"
                :key="execution.id"
                class="p-3 bg-blue-50 border border-blue-100 rounded-lg hover:bg-blue-100 hover:border-blue-200 transition-all cursor-pointer"
                @click="loadExecution(execution)"
              >
                <div class="text-xs text-gray-600 font-medium mb-1">
                  {{ new Date(execution.timestamp).toLocaleTimeString() }}
                </div>
                <div class="text-sm text-gray-900 truncate">
                  {{ truncatePrompt(execution.prompt) }}
                </div>
              </div>
            </div>
            <div v-else class="text-gray-500 italic text-center py-8">
              No execution history yet
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { gsap } from 'gsap'
import { useLlmStore } from '@/stores/llmStore'
import { usePromptStore } from '@/stores/promptStore'
import { useMetricsStore } from '@/stores/metricsStore'
import { mapProviderNameToEnum } from '@/utils/providerMapper'
import * as apiService from '@/services/api'
import type { PromptExecution } from '@/stores/promptStore'

// Constants
const CHARS_PER_TOKEN = 4 // Rough approximation for token calculation
const HISTORY_TRUNCATE_LENGTH = 60

const router = useRouter()
const llmStore = useLlmStore()
const promptStore = usePromptStore()
const metricsStore = useMetricsStore()

const promptCard = ref<HTMLElement>()
const responseCard = ref<HTMLElement>()
const metricsCard = ref<HTMLElement>()

// Helper function to truncate prompt for display
const truncatePrompt = (prompt: string) => {
  return prompt.length > HISTORY_TRUNCATE_LENGTH
    ? prompt.substring(0, HISTORY_TRUNCATE_LENGTH) + '...'
    : prompt
}

onMounted(() => {
  // Animate cards on mount with GSAP
  gsap.from([promptCard.value, responseCard.value, metricsCard.value], {
    y: 20,
    opacity: 0,
    duration: 0.6,
    stagger: 0.1,
    ease: 'power2.out'
  })
})

function changeModel() {
  router.push({ name: 'model-selection' })
}

async function executePrompt() {
  if (!promptStore.currentPrompt.trim() || !llmStore.selectedModel) return

  promptStore.setExecuting(true)
  const startTime = Date.now()
  
  try {
    // Map provider name to enum
    const providerEnum = mapProviderNameToEnum(llmStore.selectedModel.providerName)
    
    // Execute real API call
    const response = await apiService.executePrompt({
      provider: providerEnum,
      prompt: promptStore.currentPrompt,
      model: llmStore.selectedModel.modelName
    })
    
    const executionTime = Date.now() - startTime
    
    // Update stores with real data
    promptStore.setResponse(response.data.content)
    promptStore.addExecution({
      prompt: promptStore.currentPrompt,
      response: response.data.content,
      success: true
    })
    
    metricsStore.addMetrics({
      inputTokens: response.data.inputTokens,
      outputTokens: response.data.outputTokens,
      totalTokens: response.data.inputTokens + response.data.outputTokens,
      executionTimeMs: response.data.latencyMs,
      cost: response.data.cost,
      modelName: response.data.model,
      providerName: llmStore.selectedModel.providerName
    })
    
    // Animate response card
    gsap.from(responseCard.value, {
      scale: 0.98,
      opacity: 0,
      duration: 0.3,
      ease: 'power2.out'
    })
  } catch (error: any) {
    const errorMessage = error.response?.data?.detail || error.message || 'Unknown error occurred'
    promptStore.setResponse('')
    promptStore.addExecution({
      prompt: promptStore.currentPrompt,
      response: '',
      success: false,
      error: errorMessage
    })
    console.error('Error executing prompt:', error)
  } finally {
    promptStore.setExecuting(false)
  }
}

function loadExecution(execution: PromptExecution) {
  promptStore.setPrompt(execution.prompt)
  promptStore.setResponse(execution.response)
  
  // Animate to show the loaded content
  gsap.from([promptCard.value, responseCard.value], {
    scale: 0.98,
    duration: 0.3,
    ease: 'power2.out'
  })
}
</script>
