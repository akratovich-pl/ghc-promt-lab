import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export interface Provider {
  name: string
  isAvailable: boolean
  supportedModels: string[]
}

export interface Model {
  name: string
  displayName: string
  provider: string
  maxTokens: number
  inputCostPer1kTokens: number
  outputCostPer1kTokens: number
}

export interface SelectedModel {
  providerName: string
  modelName: string
  model: Model | null
}

export const useLlmStore = defineStore('llm', () => {
  // State
  const providers = ref<Provider[]>([])
  const selectedModel = ref<SelectedModel | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const isApiConnected = ref(false)
  const isCheckingConnection = ref(false)

  // Computed
  const hasSelectedModel = computed(() => selectedModel.value !== null)
  const availableProviders = computed(() => 
    providers.value.filter(p => p.isAvailable)
  )

  // Actions
  async function fetchProviders() {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<Provider[]>('/providers')
      providers.value = response.data
    } catch (err: any) {
      error.value = err.response?.data?.detail || 'Failed to load providers'
      console.error('Error fetching providers:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function selectModel(providerName: string, modelName: string) {
    loading.value = true
    error.value = null

    try {
      const response = await api.get<Model[]>(`/providers/${providerName}/models`)
      const model = response.data.find(m => m.name === modelName) || null
      
      selectedModel.value = {
        providerName,
        modelName,
        model
      }
    } catch (err: any) {
      error.value = err.response?.data?.detail || `Failed to load model ${modelName}`
      console.error('Error selecting model:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  function clearSelection() {
    selectedModel.value = null
  }

  async function checkApiConnection() {
    if (isCheckingConnection.value) return
    
    isCheckingConnection.value = true
    try {
      await api.get('/providers')
      isApiConnected.value = true
    } catch (err) {
      isApiConnected.value = false
      console.warn('API connection check failed:', err)
    } finally {
      isCheckingConnection.value = false
    }
  }

  // Start periodic connection check
  let connectionCheckInterval: number | null = null
  
  function startConnectionMonitoring() {
    // Initial check
    checkApiConnection()
    
    // Check every 15 seconds
    if (connectionCheckInterval === null) {
      connectionCheckInterval = window.setInterval(() => {
        checkApiConnection()
      }, 15000)
    }
  }

  function stopConnectionMonitoring() {
    if (connectionCheckInterval !== null) {
      clearInterval(connectionCheckInterval)
      connectionCheckInterval = null
    }
  }

  return {
    // State
    providers,
    selectedModel,
    loading,
    error,
    isApiConnected,
    isCheckingConnection,
    // Computed
    hasSelectedModel,
    availableProviders,
    // Actions
    fetchProviders,
    selectModel,
    clearSelection,
    checkApiConnection,
    startConnectionMonitoring,
    stopConnectionMonitoring
  }
}, {
  persist: true
})
