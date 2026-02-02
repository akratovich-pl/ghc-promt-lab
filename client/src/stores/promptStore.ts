import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface PromptExecution {
  id: string
  input: string
  output: string
  timestamp: Date
  status: 'idle' | 'processing' | 'completed' | 'error'
}

export const usePromptStore = defineStore('prompt', () => {
  const currentInput = ref('')
  const currentOutput = ref('')
  const executionHistory = ref<PromptExecution[]>([])
  const processingState = ref<'idle' | 'processing' | 'completed' | 'error'>('idle')

  function setInput(input: string) {
    currentInput.value = input
  }

  function setOutput(output: string) {
    currentOutput.value = output
  }

  function setProcessingState(state: 'idle' | 'processing' | 'completed' | 'error') {
    processingState.value = state
  }

  function addToHistory(execution: PromptExecution) {
    executionHistory.value.unshift(execution)
  }

  function clearHistory() {
    executionHistory.value = []
  }

  function reset() {
    currentInput.value = ''
    currentOutput.value = ''
    processingState.value = 'idle'
  }

  return {
    currentInput,
    currentOutput,
    executionHistory,
    processingState,
    setInput,
    setOutput,
    setProcessingState,
    addToHistory,
    clearHistory,
    reset
  }
})
