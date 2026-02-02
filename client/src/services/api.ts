import axios from 'axios'
import type {
  ProviderInfoResponse,
  ModelInfoResponse,
  ExecutePromptRequest,
  ExecutePromptResponse,
  EstimateTokensRequest,
  EstimateTokensResponse
} from '@/types/api'

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000 // 30 second timeout
})

// Add request interceptor for logging
api.interceptors.request.use(
  (config) => {
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`)
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Add response interceptor for error handling
api.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response) {
      // Server responded with error status
      console.error('API Error:', error.response.status, error.response.data)
    } else if (error.request) {
      // Request made but no response
      console.error('API Error: No response received')
    } else {
      // Error in request setup
      console.error('API Error:', error.message)
    }
    return Promise.reject(error)
  }
)

// Provider endpoints
export const getProviders = () => {
  return api.get<ProviderInfoResponse[]>('/providers')
}

export const getProviderModels = (providerName: string) => {
  return api.get<ModelInfoResponse[]>(`/providers/${providerName}/models`)
}

// Prompt endpoints
export const executePrompt = (request: ExecutePromptRequest) => {
  return api.post<ExecutePromptResponse>('/prompts/execute', request)
}

export const estimateTokens = (request: EstimateTokensRequest) => {
  return api.post<EstimateTokensResponse>('/prompts/estimate', request)
}

export default api
