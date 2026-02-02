import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ExecutePromptResponse, EstimateTokensResponse } from '@/types/api'
import api from '@/services/api'

export const usePromptStore = defineStore('prompt', () => {
  // State
  const currentPrompt = ref<string>('')
  const systemPrompt = ref<string>('')
  const response = ref<ExecutePromptResponse | null>(null)
  const tokenEstimate = ref<EstimateTokensResponse | null>(null)
  const loading = ref(false)
  const executing = ref(false)
  const estimating = ref(false)
  const error = ref<string | null>(null)
  const conversationId = ref<string | null>(null)

  // Actions
  const executePrompt = async (
    prompt: string,
    model?: string,
    systemPromptText?: string,
    maxTokens?: number,
    temperature?: number
  ) => {
    executing.value = true
    loading.value = true
    error.value = null
    
    try {
      const requestData = {
        prompt,
        systemPrompt: systemPromptText,
        conversationId: conversationId.value,
        model,
        maxTokens,
        temperature
      }
      
      const res = await api.post<ExecutePromptResponse>('/prompts/execute', requestData)
      response.value = res.data
      currentPrompt.value = prompt
      
      return res.data
    } catch (err: any) {
      error.value = err.response?.data?.detail || 'Failed to execute prompt'
      console.error('Error executing prompt:', err)
      throw err
    } finally {
      executing.value = false
      loading.value = false
    }
  }

  const estimateTokens = async (prompt: string, model?: string) => {
    estimating.value = true
    error.value = null
    
    try {
      const requestData = {
        prompt,
        model
      }
      
      const res = await api.post<EstimateTokensResponse>('/prompts/estimate', requestData)
      tokenEstimate.value = res.data
      
      return res.data
    } catch (err: any) {
      error.value = err.response?.data?.detail || 'Failed to estimate tokens'
      console.error('Error estimating tokens:', err)
    } finally {
      estimating.value = false
    }
  }

  const clearResponse = () => {
    response.value = null
    error.value = null
  }

  const clearTokenEstimate = () => {
    tokenEstimate.value = null
  }

  const setCurrentPrompt = (prompt: string) => {
    currentPrompt.value = prompt
  }

  const setSystemPrompt = (prompt: string) => {
    systemPrompt.value = prompt
  }

  const setConversationId = (id: string | null) => {
    conversationId.value = id
  }

  return {
    // State
    currentPrompt,
    systemPrompt,
    response,
    tokenEstimate,
    loading,
    executing,
    estimating,
    error,
    conversationId,
    // Actions
    executePrompt,
    estimateTokens,
    clearResponse,
    clearTokenEstimate,
    setCurrentPrompt,
    setSystemPrompt,
    setConversationId
  }
})
