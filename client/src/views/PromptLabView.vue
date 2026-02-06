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
            <div 
              v-if="promptStore.currentResponse"
              class="prose prose-sm max-w-none p-4 bg-gray-50 rounded-lg min-h-[200px] text-gray-900 border border-gray-200 markdown-response"
              v-html="renderMarkdown(promptStore.currentResponse)"
            ></div>
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
              <AppTooltip
                :content="getConceptTooltip('prompt')"
                position="left"
              >
                <div class="flex justify-between items-center py-2 border-b border-gray-100 cursor-help">
                  <span class="text-gray-700 font-medium inline-flex items-center gap-1">
                    Total Executions
                    <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </span>
                  <span class="font-bold text-gray-900 text-lg">
                    {{ metricsStore.totalExecutions }}
                  </span>
                </div>
              </AppTooltip>
              <AppTooltip
                :content="getConceptTooltip('tokens')"
                position="left"
              >
                <div class="flex justify-between items-center py-2 border-b border-gray-100 cursor-help">
                  <span class="text-gray-700 font-medium inline-flex items-center gap-1">
                    Total Tokens
                    <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </span>
                  <span class="font-bold text-gray-900 text-lg">
                    {{ metricsStore.totalTokensUsed.toLocaleString() }}
                  </span>
                </div>
              </AppTooltip>
              <div class="flex justify-between items-center py-2 border-b border-gray-100">
                <span class="text-gray-700 font-medium">Total Cost</span>
                <span class="font-bold text-accent-green text-lg">
                  ${{ metricsStore.totalCost.toFixed(4) }}
                </span>
              </div>
              <AppTooltip
                :content="getConceptTooltip('latency')"
                position="left"
              >
                <div class="flex justify-between items-center py-2 cursor-help">
                  <span class="text-gray-700 font-medium inline-flex items-center gap-1">
                    Avg Time
                    <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </span>
                  <span class="font-bold text-gray-900 text-lg">
                    {{ (metricsStore.averageExecutionTime / 1000).toFixed(2) }}s
                  </span>
                </div>
              </AppTooltip>
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
/* No additional styles needed - gradient animation moved to AppHeader component */
</style>
