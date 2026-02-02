<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 p-6">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-4xl font-bold text-gray-900 dark:text-white mb-2">
          🧪 Prompt Lab
        </h1>
        <p class="text-gray-600 dark:text-gray-400">
          Test and visualize AI prompt execution with real-time metrics
        </p>
      </div>

      <!-- Main Layout -->
      <div class="relative">
        <!-- Grid Layout for Blocks -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 lg:gap-12 mb-8">
          <!-- Input Block -->
          <div ref="inputBlockRef">
            <InputBlock
              v-model="promptInput"
              :is-active="processingState === 'idle'"
              :disabled="processingState === 'processing'"
              @input="handleInputChange"
            />
          </div>

          <!-- Processor Block -->
          <div ref="processorBlockRef">
            <ProcessorBlock
              :state="processingState"
              :llm-name="currentLlm?.name"
              :llm-model="currentLlm?.model"
              :input-tokens="metrics.inputTokens"
              :output-tokens="metrics.outputTokens"
              :error-message="errorMessage"
            />
          </div>

          <!-- Output Block -->
          <div ref="outputBlockRef">
            <OutputBlock
              :output-text="promptOutput"
              :is-loading="processingState === 'processing'"
              :show-metrics="true"
              :metrics="{
                inputTokens: metrics.inputTokens,
                outputTokens: metrics.outputTokens,
                duration: metrics.duration,
                totalCost: metrics.totalCost
              }"
            />
          </div>
        </div>

        <!-- Connection Lines -->
        <ConnectionLine
          v-if="showConnections"
          :x="line1.x"
          :y="line1.y"
          :width="line1.width"
          :height="line1.height"
          :start-x="10"
          :start-y="line1.height / 2"
          :end-x="line1.width - 10"
          :end-y="line1.height / 2"
          :animated="processingState === 'processing'"
          start-color="#0ea5e9"
          end-color="#06b6d4"
          flow-color="#38bdf8"
        />

        <ConnectionLine
          v-if="showConnections"
          :x="line2.x"
          :y="line2.y"
          :width="line2.width"
          :height="line2.height"
          :start-x="10"
          :start-y="line2.height / 2"
          :end-x="line2.width - 10"
          :end-y="line2.height / 2"
          :animated="processingState === 'processing' || processingState === 'completed'"
          start-color="#06b6d4"
          end-color="#10b981"
          flow-color="#34d399"
        />

        <!-- Action Buttons -->
        <div class="flex justify-center gap-4">
          <button
            @click="executePrompt"
            :disabled="!canExecute"
            class="px-8 py-3 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 
                   text-white font-semibold rounded-lg shadow-lg 
                   transition-all duration-200 transform hover:scale-105 
                   disabled:cursor-not-allowed disabled:transform-none"
          >
            {{ processingState === 'processing' ? '⚡ Processing...' : '🚀 Execute Prompt' }}
          </button>

          <button
            v-if="processingState !== 'idle'"
            @click="reset"
            class="px-6 py-3 bg-gray-600 hover:bg-gray-700 text-white 
                   font-semibold rounded-lg shadow-lg transition-colors duration-200"
          >
            🔄 Reset
          </button>
        </div>

        <!-- Summary Stats -->
        <div v-if="promptOutput" class="mt-8 p-6 bg-white dark:bg-gray-800 rounded-lg shadow-lg">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            📊 Execution Summary
          </h3>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div class="text-center">
              <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                {{ metrics.inputTokens + metrics.outputTokens }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Total Tokens</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-green-600 dark:text-green-400">
                {{ formatDuration(metrics.duration) }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Duration</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-purple-600 dark:text-purple-400">
                ${{ metrics.totalCost.toFixed(6) }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Estimated Cost</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-pink-600 dark:text-pink-400">
                {{ promptInput.length }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">Characters</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useLlmStore } from '@/stores/llmStore'
import { usePromptStore } from '@/stores/promptStore'
import { useMetricsStore } from '@/stores/metricsStore'
import InputBlock from '@/components/prompt/InputBlock.vue'
import ProcessorBlock from '@/components/prompt/ProcessorBlock.vue'
import OutputBlock from '@/components/prompt/OutputBlock.vue'
import ConnectionLine from '@/components/prompt/ConnectionLine.vue'

// Stores
const llmStore = useLlmStore()
const promptStore = usePromptStore()
const metricsStore = useMetricsStore()

// Refs for block positions
const inputBlockRef = ref<HTMLDivElement>()
const processorBlockRef = ref<HTMLDivElement>()
const outputBlockRef = ref<HTMLDivElement>()

// State
const promptInput = ref('')
const promptOutput = ref('')
const processingState = ref<'idle' | 'processing' | 'completed' | 'error'>('idle')
const errorMessage = ref('')
const showConnections = ref(false)

// Connection line positions
const line1 = ref({ x: 0, y: 0, width: 0, height: 0 })
const line2 = ref({ x: 0, y: 0, width: 0, height: 0 })

// Computed
const currentLlm = computed(() => llmStore.selectedProvider || llmStore.availableProviders[0])

const metrics = computed(() => ({
  inputTokens: metricsStore.inputTokens,
  outputTokens: metricsStore.outputTokens,
  duration: metricsStore.duration,
  totalCost: metricsStore.totalCost
}))

const canExecute = computed(() => {
  return promptInput.value.trim().length > 0 && 
         processingState.value !== 'processing' &&
         currentLlm.value !== null
})

// Methods
function handleInputChange(value: string) {
  promptStore.setInput(value)
  // Estimate tokens for input
  metricsStore.setInputTokens(Math.ceil(value.length / 4))
}

async function executePrompt() {
  if (!canExecute.value) return

  processingState.value = 'processing'
  promptStore.setProcessingState('processing')
  metricsStore.startTimer()
  errorMessage.value = ''
  
  try {
    // Simulate API call with timeout
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    // Mock response
    const mockResponse = `This is a mock response to your prompt: "${promptInput.value.substring(0, 50)}..."
    
The system is functioning correctly with GSAP animations, Tailwind CSS styling, and component state management.

Key features demonstrated:
- Real-time token counting
- Processing state visualization
- Animated connection lines
- Responsive layout
- Metrics tracking`

    promptOutput.value = mockResponse
    promptStore.setOutput(mockResponse)
    metricsStore.setOutputTokens(Math.ceil(mockResponse.length / 4))
    metricsStore.stopTimer()
    
    processingState.value = 'completed'
    promptStore.setProcessingState('completed')
    
    // Add to history
    promptStore.addToHistory({
      id: Date.now().toString(),
      input: promptInput.value,
      output: mockResponse,
      timestamp: new Date(),
      status: 'completed'
    })
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unknown error occurred'
    processingState.value = 'error'
    promptStore.setProcessingState('error')
    metricsStore.stopTimer()
  }
}

function reset() {
  promptInput.value = ''
  promptOutput.value = ''
  processingState.value = 'idle'
  errorMessage.value = ''
  promptStore.reset()
  metricsStore.reset()
}

function formatDuration(ms: number): string {
  if (ms < 1000) return `${ms}ms`
  return `${(ms / 1000).toFixed(2)}s`
}

function updateConnectionLines() {
  if (!inputBlockRef.value || !processorBlockRef.value || !outputBlockRef.value) {
    return
  }

  const inputRect = inputBlockRef.value.getBoundingClientRect()
  const processorRect = processorBlockRef.value.getBoundingClientRect()
  const outputRect = outputBlockRef.value.getBoundingClientRect()

  // Line 1: Input to Processor
  line1.value = {
    x: inputRect.right,
    y: inputRect.top + inputRect.height / 2 - 25,
    width: processorRect.left - inputRect.right,
    height: 50
  }

  // Line 2: Processor to Output
  line2.value = {
    x: processorRect.right,
    y: processorRect.top + processorRect.height / 2 - 25,
    width: outputRect.left - processorRect.right,
    height: 50
  }

  showConnections.value = window.innerWidth >= 1024 // Only show on desktop
}

let resizeObserver: ResizeObserver | null = null

onMounted(async () => {
  // Select default LLM if none selected
  if (!llmStore.selectedProvider && llmStore.availableProviders.length > 0) {
    llmStore.selectProvider(llmStore.availableProviders[0])
  }

  await nextTick()
  
  // Update connection lines after mount
  setTimeout(updateConnectionLines, 100)

  // Watch for window resize
  window.addEventListener('resize', updateConnectionLines)

  // Use ResizeObserver for block size changes
  resizeObserver = new ResizeObserver(updateConnectionLines)
  if (inputBlockRef.value) resizeObserver.observe(inputBlockRef.value)
  if (processorBlockRef.value) resizeObserver.observe(processorBlockRef.value)
  if (outputBlockRef.value) resizeObserver.observe(outputBlockRef.value)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateConnectionLines)
  if (resizeObserver) {
    resizeObserver.disconnect()
  }
})
</script>
