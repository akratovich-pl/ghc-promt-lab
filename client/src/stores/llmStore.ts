import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

// Mock cost values for testing without backend API
const MOCK_INPUT_COST_PER_1K = 0.00025
const MOCK_OUTPUT_COST_PER_1K = 0.0005

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
      // Fallback to mock data for development/testing
      console.warn('API not available, using mock data:', err)
      providers.value = [
        {
          name: 'Google',
          isAvailable: true,
          supportedModels: ['gemini-pro', 'gemini-pro-vision']
        },
        {
          name: 'OpenAI',
          isAvailable: true,
          supportedModels: ['gpt-4', 'gpt-3.5-turbo']
        },
        {
          name: 'Anthropic',
          isAvailable: true,
          supportedModels: ['claude-3-opus', 'claude-3-sonnet']
        }
      ]
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
      // Fallback to mock model data for development/testing
      console.warn('API not available, using mock model data:', err)
      selectedModel.value = {
        providerName,
        modelName,
        model: {
          name: modelName,
          displayName: modelName,
          provider: providerName,
          maxTokens: 8192,
          inputCostPer1kTokens: MOCK_INPUT_COST_PER_1K,
          outputCostPer1kTokens: MOCK_OUTPUT_COST_PER_1K
        }
      }
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
