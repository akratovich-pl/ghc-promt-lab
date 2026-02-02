import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface PromptExecution {
  id: string
  timestamp: Date
  prompt: string
  response: string
  success: boolean
  error?: string
}

export const usePromptStore = defineStore('prompt', () => {
  // State
  const currentPrompt = ref('')
  const currentResponse = ref('')
  const executionHistory = ref<PromptExecution[]>([])
  const isExecuting = ref(false)

  // Computed
  const hasHistory = computed(() => executionHistory.value.length > 0)
  const lastExecution = computed(() => 
    executionHistory.value.length > 0 
      ? executionHistory.value[0] 
      : null
  )

  // Actions
  function setPrompt(prompt: string) {
    currentPrompt.value = prompt
  }

  function setResponse(response: string) {
    currentResponse.value = response
  }

  function addExecution(execution: Omit<PromptExecution, 'id' | 'timestamp'>) {
    const newExecution: PromptExecution = {
      id: crypto.randomUUID(),
      timestamp: new Date(),
      ...execution
    }
    
    // Add to beginning of array (most recent first)
    executionHistory.value.unshift(newExecution)
    
    // Keep only last 50 executions
    if (executionHistory.value.length > 50) {
      executionHistory.value = executionHistory.value.slice(0, 50)
    }
  }

  function clearHistory() {
    executionHistory.value = []
  }

  function clearCurrent() {
    currentPrompt.value = ''
    currentResponse.value = ''
  }

  function setExecuting(value: boolean) {
    isExecuting.value = value
  }

  return {
    // State
    currentPrompt,
    currentResponse,
    executionHistory,
    isExecuting,
    // Computed
    hasHistory,
    lastExecution,
    // Actions
    setPrompt,
    setResponse,
    addExecution,
    clearHistory,
    clearCurrent,
    setExecuting
  }
}, {
  persist: true
})
