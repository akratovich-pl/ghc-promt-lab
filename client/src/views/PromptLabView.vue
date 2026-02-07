<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-white to-gray-50">
    <!-- Header -->
    <AppHeader :use-animation="true">
      <template #navigation>
        <p class="text-sm text-gray-600 font-medium">
          {{ llmStore.selectedModel?.providerName }} - {{ llmStore.selectedModel?.modelName }}
        </p>
        <button
          @click="changeModel"
          class="px-5 py-2.5 bg-blue-100 hover:bg-blue-200 text-blue-800 font-medium rounded-lg transition-colors"
        >
          Change Model
        </button>
      </template>
    </AppHeader>

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
      <!-- API Connection Status Banner -->
      <div 
        v-if="!llmStore.isApiConnected"
        class="mb-6 bg-red-50 border-l-4 border-red-500 p-4 rounded-lg shadow-md"
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
            <div class="mt-2 text-sm text-red-700">
              <p>The backend service is currently unavailable. Please check that:</p>
              <ul class="list-disc list-inside mt-1 ml-2 space-y-1">
                <li>The API server is running</li>
                <li>The server is accessible at the configured endpoint</li>
                <li>Your network connection is active</li>
              </ul>
              <p class="mt-2 text-xs text-red-600">
                Automatically checking connection every 15 seconds...
              </p>
            </div>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Left Column: Prompt Input -->
        <div class="lg:col-span-2 space-y-6">
          <!-- Prompt Input Card -->
          <div 
            ref="promptCard"
            class="bg-white rounded-xl shadow-md border border-gray-200 p-6"
            :class="{ 'opacity-60': !llmStore.isApiConnected }"
          >
            <h2 class="text-xl font-semibold text-gray-900 mb-4">
              Prompt Input
            </h2>
            <div class="relative">
              <textarea
                v-model="promptStore.currentPrompt"
                :disabled="!llmStore.isApiConnected"
                placeholder="Enter your prompt here..."
                rows="8"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-200 focus:border-blue-400 focus:outline-none bg-white text-gray-900 placeholder-gray-400 resize-none transition-colors disabled:bg-gray-100 disabled:cursor-not-allowed"
              ></textarea>
              <!-- Overlay message when API is disconnected -->
              <div
                v-if="!llmStore.isApiConnected"
                class="absolute inset-0 flex items-center justify-center bg-gray-50 bg-opacity-90 rounded-lg pointer-events-none"
              >
                <div class="text-center px-4">
                  <svg class="w-12 h-12 text-gray-400 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21M15.536 8.464a5 5 0 010 7.072m0 0l-2.829-2.829m-4.243 2.829a4.978 4.978 0 01-1.414-2.83m-1.414 5.658a9 9 0 01-2.167-9.238m7.824 2.167a1 1 0 111.414 1.414m-1.414-1.414L3 3m8.293 8.293l1.414 1.414" />
                  </svg>
                  <p class="text-gray-600 font-medium">
                    API connection required
                  </p>
                </div>
              </div>
            </div>
            <div class="mt-4 flex items-center justify-between">
              <AppTooltip
                :content="getConceptTooltip('tokens')"
                position="top"
              >
                <div class="text-sm text-gray-600 font-medium cursor-help inline-flex items-center gap-1">
                  Characters: {{ promptStore.currentPrompt.length }}
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </AppTooltip>
              <button
                @click="executePrompt"
                :disabled="!llmStore.isApiConnected || !promptStore.currentPrompt.trim() || promptStore.isExecuting"
                :title="!llmStore.isApiConnected ? 'Backend API is unavailable' : ''"
                class="px-6 py-3 bg-blue-100 hover:bg-blue-200 disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed text-blue-800 font-semibold rounded-lg transition-colors"
              >
                {{ !llmStore.isApiConnected ? 'API Unavailable' : (promptStore.isExecuting ? 'Executing...' : 'Execute') }}
              </button>
            </div>
          </div>

          <!-- Response Card -->
          <div 
            ref="responseCard"
            class="bg-white rounded-xl shadow-md border border-gray-200 p-6"
          >
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-xl font-semibold text-gray-900">
                Response
              </h2>
              <button
                v-if="promptStore.currentResponse"
                @click="clearResponse"
                class="text-sm px-4 py-2 bg-blue-100 hover:bg-blue-200 text-blue-800 font-medium rounded-lg transition-colors"
              >
                Clear
              </button>
            </div>
            <!-- Loading State with Animated Pulse -->
            <div 
              v-if="promptStore.isExecuting"
              class="p-4 bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg min-h-[200px] flex flex-col items-center justify-center border border-blue-200"
            >
              <div class="relative">
                <!-- Animated Heartbeat Icon -->
                <svg 
                  class="w-16 h-16 text-blue-500 animate-heartbeat"
                  fill="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                </svg>
                <!-- Pulse Rings -->
                <div class="absolute inset-0 flex items-center justify-center">
                  <div class="w-16 h-16 bg-blue-400 rounded-full opacity-75 animate-ping"></div>
                </div>
                <div class="absolute inset-0 flex items-center justify-center animation-delay-150">
                  <div class="w-16 h-16 bg-blue-300 rounded-full opacity-50 animate-ping"></div>
                </div>
              </div>
              <p class="mt-6 text-blue-700 font-medium text-lg">Generating response...</p>
              <p class="mt-2 text-blue-600 text-sm">Please wait while AI processes your request</p>
              
              <!-- Progress Bar -->
              <div class="w-full max-w-md mt-6">
                <div class="h-2 bg-blue-200 rounded-full overflow-hidden">
                  <div class="h-full bg-gradient-to-r from-blue-500 via-indigo-500 to-purple-500 rounded-full animate-progress"></div>
                </div>
              </div>
            </div>
            
            <!-- Response Content -->
            <div 
              v-else-if="promptStore.currentResponse"
              class="prose prose-sm max-w-none p-4 bg-gray-50 rounded-lg min-h-[200px] text-gray-900 border border-gray-200 markdown-response"
              v-html="renderMarkdown(promptStore.currentResponse)"
            ></div>
            
            <!-- Empty State -->
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
                <span class="text-gray-700 font-medium inline-flex items-center gap-1">
                  Total Executions
                  <AppTooltip
                    :content="getConceptTooltip('prompt')"
                    position="top"
                  >
                    <svg class="w-4 h-4 text-gray-400 cursor-help" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </AppTooltip>
                </span>
                <span class="font-bold text-gray-900 text-lg">
                  {{ metricsStore.totalExecutions }}
                </span>
              </div>
              <div class="flex justify-between items-center py-2 border-b border-gray-100">
                <span class="text-gray-700 font-medium inline-flex items-center gap-1">
                  Total Tokens
                  <AppTooltip
                    :content="getConceptTooltip('tokens')"
                    position="top"
                  >
                    <svg class="w-4 h-4 text-gray-400 cursor-help" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </AppTooltip>
                </span>
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
                <span class="text-gray-700 font-medium inline-flex items-center gap-1">
                  Avg Time
                  <AppTooltip
                    :content="getConceptTooltip('latency')"
                    position="top"
                  >
                    <svg class="w-4 h-4 text-gray-400 cursor-help" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </AppTooltip>
                </span>
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
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { gsap } from 'gsap'
import { useLlmStore } from '@/stores/llmStore'
import { usePromptStore } from '@/stores/promptStore'
import { useMetricsStore } from '@/stores/metricsStore'
import { mapProviderNameToEnum } from '@/utils/providerMapper'
import * as apiService from '@/services/api'
import type { PromptExecution } from '@/stores/promptStore'
import AppHeader from '@/components/common/AppHeader.vue'
import AppTooltip from '@/components/common/AppTooltip.vue'
import { useMarkdown } from '@/composables/useMarkdown'
import { useTooltip } from '@/composables/useTooltip'

// Constants
const HISTORY_TRUNCATE_LENGTH = 60

const { renderMarkdown } = useMarkdown()
const { getTooltipContent } = useTooltip()

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
  
  // Start monitoring API connection
  llmStore.startConnectionMonitoring()
})

onUnmounted(() => {
  // Stop monitoring when component is destroyed
  llmStore.stopConnectionMonitoring()
})

function changeModel() {
  router.push({ name: 'model-selection' })
}

function clearResponse() {
  promptStore.setResponse('')
  
  // Subtle fade animation
  if (responseCard.value) {
    gsap.from(responseCard.value, {
      opacity: 0,
      duration: 0.2,
      ease: 'power2.out'
    })
  }
}

async function executePrompt() {
  if (!promptStore.currentPrompt.trim() || !llmStore.selectedModel) return

  promptStore.setExecuting(true)
  
  try {
    // Map provider name to enum
    const providerEnum = mapProviderNameToEnum(llmStore.selectedModel.providerName)
    
    // Execute real API call
    const response = await apiService.executePrompt({
      provider: providerEnum,
      prompt: promptStore.currentPrompt,
      model: llmStore.selectedModel.modelName
    })
    
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
    if (responseCard.value) {
      gsap.from(responseCard.value, {
        scale: 0.98,
        opacity: 0,
        duration: 0.3,
        ease: 'power2.out'
      })
    }
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

// Get tooltip for concepts
function getConceptTooltip(key: string) {
  return getTooltipContent('concepts', key)
}
</script>

<style scoped>
/* Heartbeat animation */
@keyframes heartbeat {
  0%, 100% {
    transform: scale(1);
  }
  10%, 30% {
    transform: scale(1.1);
  }
  20%, 40% {
    transform: scale(1);
  }
}

.animate-heartbeat {
  animation: heartbeat 1.5s ease-in-out infinite;
}

/* Progress bar animation */
@keyframes progress {
  0% {
    transform: translateX(-100%);
  }
  100% {
    transform: translateX(100%);
  }
}

.animate-progress {
  animation: progress 1.5s ease-in-out infinite;
}

/* Animation delay utility */
.animation-delay-150 {
  animation-delay: 150ms;
}
</style>
