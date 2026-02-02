import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface TokenMetrics {
  inputTokens: number
  outputTokens: number
  totalTokens: number
}

export interface TimingMetrics {
  startTime: number | null
  endTime: number | null
  duration: number
}

export interface CostMetrics {
  inputCost: number
  outputCost: number
  totalCost: number
}

export const useMetricsStore = defineStore('metrics', () => {
  const inputTokens = ref(0)
  const outputTokens = ref(0)
  const startTime = ref<number | null>(null)
  const endTime = ref<number | null>(null)
  const inputCostPer1K = ref(0.00025) // Default for Gemini
  const outputCostPer1K = ref(0.0005) // Default for Gemini

  const totalTokens = computed(() => inputTokens.value + outputTokens.value)
  
  const duration = computed(() => {
    if (startTime.value && endTime.value) {
      return endTime.value - startTime.value
    }
    return 0
  })

  const inputCost = computed(() => (inputTokens.value / 1000) * inputCostPer1K.value)
  const outputCost = computed(() => (outputTokens.value / 1000) * outputCostPer1K.value)
  const totalCost = computed(() => inputCost.value + outputCost.value)

  function setInputTokens(tokens: number) {
    inputTokens.value = tokens
  }

  function setOutputTokens(tokens: number) {
    outputTokens.value = tokens
  }

  function startTimer() {
    startTime.value = Date.now()
    endTime.value = null
  }

  function stopTimer() {
    endTime.value = Date.now()
  }

  function setCostRates(input: number, output: number) {
    inputCostPer1K.value = input
    outputCostPer1K.value = output
  }

  function reset() {
    inputTokens.value = 0
    outputTokens.value = 0
    startTime.value = null
    endTime.value = null
  }

  return {
    inputTokens,
    outputTokens,
    totalTokens,
    startTime,
    endTime,
    duration,
    inputCost,
    outputCost,
    totalCost,
    setInputTokens,
    setOutputTokens,
    startTimer,
    stopTimer,
    setCostRates,
    reset
  }
})
