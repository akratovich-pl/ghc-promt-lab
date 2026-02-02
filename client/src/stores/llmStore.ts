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
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch providers'
      console.error('Error fetching providers:', err)
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
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to select model'
      console.error('Error selecting model:', err)
    } finally {
      loading.value = false
    }
  }

  function clearSelection() {
    selectedModel.value = null
  }

  return {
    // State
    providers,
    selectedModel,
    loading,
    error,
    // Computed
    hasSelectedModel,
    availableProviders,
    // Actions
    fetchProviders,
    selectModel,
    clearSelection
  }
}, {
  persist: true
})
