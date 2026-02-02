import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface LlmProvider {
  id: string
  name: string
  model: string
  enabled: boolean
}

export const useLlmStore = defineStore('llm', () => {
  const selectedProvider = ref<LlmProvider | null>(null)
  const availableProviders = ref<LlmProvider[]>([
    {
      id: 'google-gemini',
      name: 'Google Gemini',
      model: 'gemini-pro',
      enabled: true
    }
  ])

  function selectProvider(provider: LlmProvider) {
    selectedProvider.value = provider
  }

  function clearProvider() {
    selectedProvider.value = null
  }

  return {
    selectedProvider,
    availableProviders,
    selectProvider,
    clearProvider
  }
})
