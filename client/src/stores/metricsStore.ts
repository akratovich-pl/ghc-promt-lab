import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface ExecutionMetrics {
  id: string
  timestamp: Date
  inputTokens: number
  outputTokens: number
  totalTokens: number
  executionTimeMs: number
  cost: number
  modelName: string
  providerName: string
}

export const useMetricsStore = defineStore('metrics', () => {
  // State
  const currentMetrics = ref<Partial<ExecutionMetrics>>({})
  const metricsHistory = ref<ExecutionMetrics[]>([])

  // Computed
  const totalExecutions = computed(() => metricsHistory.value.length)
  
  const totalCost = computed(() => 
    metricsHistory.value.reduce((sum, m) => sum + m.cost, 0)
  )
  
  const totalTokensUsed = computed(() => 
    metricsHistory.value.reduce((sum, m) => sum + m.totalTokens, 0)
  )
  
  const averageExecutionTime = computed(() => {
    if (metricsHistory.value.length === 0) return 0
    const sum = metricsHistory.value.reduce((sum, m) => sum + m.executionTimeMs, 0)
    return sum / metricsHistory.value.length
  })

  const averageCost = computed(() => {
    if (metricsHistory.value.length === 0) return 0
    return totalCost.value / metricsHistory.value.length
  })

  // Actions
  function updateCurrentMetrics(metrics: Partial<ExecutionMetrics>) {
    currentMetrics.value = { ...currentMetrics.value, ...metrics }
  }

  function addMetrics(metrics: Omit<ExecutionMetrics, 'id' | 'timestamp'>) {
    const newMetrics: ExecutionMetrics = {
      id: crypto.randomUUID(),
      timestamp: new Date(),
      ...metrics
    }
    
    // Add to beginning of array (most recent first)
    metricsHistory.value.unshift(newMetrics)
    
    // Keep only last 100 metrics
    if (metricsHistory.value.length > 100) {
      metricsHistory.value = metricsHistory.value.slice(0, 100)
    }
  }

  function clearCurrentMetrics() {
    currentMetrics.value = {}
  }

  function clearHistory() {
    metricsHistory.value = []
  }

  function calculateCost(
    inputTokens: number, 
    outputTokens: number,
    inputCostPer1k: number,
    outputCostPer1k: number
  ): number {
    const inputCost = (inputTokens / 1000) * inputCostPer1k
    const outputCost = (outputTokens / 1000) * outputCostPer1k
    return inputCost + outputCost
  }

  return {
    // State
    currentMetrics,
    metricsHistory,
    // Computed
    totalExecutions,
    totalCost,
    totalTokensUsed,
    averageExecutionTime,
    averageCost,
    // Actions
    updateCurrentMetrics,
    addMetrics,
    clearCurrentMetrics,
    clearHistory,
    calculateCost
  }
}, {
  persist: true
})
