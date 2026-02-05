// API Types - matching backend models exactly

// Backend enum AiProvider
export enum AiProvider {
  Google = 1,
  Groq = 2
}

export interface ProviderInfoResponse {
  name: string
  isAvailable: boolean
  supportedModels: string[]
}

export interface ModelInfoResponse {
  name: string
  displayName: string
  provider: string
  maxTokens: number
  inputCostPer1kTokens: number
  outputCostPer1kTokens: number
}

export interface ProviderStatusResponse {
  name: string
  isHealthy: boolean
  errorMessage?: string
  lastChecked: string
}

export interface ExecutePromptRequest {
  provider: AiProvider  // Required field!
  prompt: string
  systemPrompt?: string
  conversationId?: string
  contextFileIds?: string[]
  model?: string
  maxTokens?: number
  temperature?: number
}

export interface ExecutePromptResponse {
  id: string
  content: string
  inputTokens: number
  outputTokens: number
  cost: number
  latencyMs: number
  model: string
  createdAt: string
}

export interface EstimateTokensRequest {
  prompt: string
  model?: string
}

export interface EstimateTokensResponse {
  tokenCount: number
  estimatedCost: number
  model: string
}

export interface ApiError {
  status: number
  title: string
  detail: string
}
