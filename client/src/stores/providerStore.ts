import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ProviderInfoResponse, ModelInfoResponse } from '@/types/api'
import api from '@/services/api'

export const useProviderStore = defineStore('provider', () => {
  // State
  const providers = ref<ProviderInfoResponse[]>([])
  const selectedProvider = ref<string>('')
  const selectedModel = ref<string>('')
  const models = ref<ModelInfoResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Actions
  const fetchProviders = async () => {
    loading.value = true
    error.value = null
    try {
      const response = await api.get<ProviderInfoResponse[]>('/providers')
      providers.value = response.data
      
      // Auto-select first available provider
      const available = providers.value.find(p => p.isAvailable)
      if (available) {
        selectedProvider.value = available.name
        // Auto-select first model if available
        if (available.supportedModels.length > 0) {
          selectedModel.value = available.supportedModels[0]
        }
      }
    } catch (err: any) {
      error.value = err.response?.data?.detail || 'Failed to load providers'
      console.error('Error fetching providers:', err)
    } finally {
      loading.value = false
    }
  }

  const fetchModels = async (providerName: string) => {
    loading.value = true
    error.value = null
    try {
      const response = await api.get<ModelInfoResponse[]>(`/providers/${providerName}/models`)
      models.value = response.data
    } catch (err: any) {
      error.value = err.response?.data?.detail || `Failed to load models for ${providerName}`
      console.error('Error fetching models:', err)
    } finally {
      loading.value = false
    }
  }

  const setSelectedProvider = (providerName: string) => {
    selectedProvider.value = providerName
    const provider = providers.value.find(p => p.name === providerName)
    if (provider && provider.supportedModels.length > 0) {
      selectedModel.value = provider.supportedModels[0]
    }
  }

  const setSelectedModel = (modelName: string) => {
    selectedModel.value = modelName
  }

  return {
    // State
    providers,
    selectedProvider,
    selectedModel,
    models,
    loading,
    error,
    // Actions
    fetchProviders,
    fetchModels,
    setSelectedProvider,
    setSelectedModel
  }
})
