import type { TooltipCategory } from './types'

/**
 * Tooltip content for LLM parameters
 * Content sourced from official provider documentation
 */
export const parameterTooltips: TooltipCategory = {
  temperature: {
    title: 'Temperature',
    description:
      'Controls randomness in responses. Lower values (0.0-0.3) make output more focused and deterministic. Higher values (0.7-1.0) make it more creative and varied.',
    example: 'Use 0.2 for code generation or factual answers, 0.8 for creative writing',
    learnMoreUrl: 'https://platform.openai.com/docs/api-reference/chat/create#temperature',
    bestPractice: 'Start with 0.7 for balanced results, then adjust based on your needs'
  },

  topP: {
    title: 'Top P (Nucleus Sampling)',
    description:
      'Controls diversity by limiting token selection. The model considers only the most probable tokens whose cumulative probability adds up to Top P. Lower values make output more focused.',
    example: 'Use 0.1 for precise answers, 0.9 for more diverse responses',
    learnMoreUrl: 'https://platform.openai.com/docs/api-reference/chat/create#top_p',
    bestPractice:
      'Use either temperature OR top_p, not both. Most providers recommend adjusting temperature first.'
  },

  maxTokens: {
    title: 'Max Tokens',
    description:
      'Maximum number of tokens (words/word pieces) the model can generate in the response. Controls output length and affects cost. One token ≈ 4 characters in English.',
    example: '100 tokens ≈ 75 words, 500 tokens ≈ 375 words, 2000 tokens ≈ 1500 words',
    learnMoreUrl: 'https://platform.openai.com/docs/api-reference/chat/create#max_tokens',
    bestPractice:
      'Set higher for detailed responses, lower for concise answers. Remember: longer responses = higher cost.'
  },

  topK: {
    title: 'Top K',
    description:
      'Limits the number of highest probability tokens considered at each step. Lower values make output more focused and deterministic.',
    example: 'Use 10-20 for focused responses, 40+ for more creative outputs',
    learnMoreUrl: 'https://ai.google.dev/gemini-api/docs/models/generative-models',
    bestPractice: 'Available in Google Gemini models. Combine with temperature for fine control.'
  },

  frequencyPenalty: {
    title: 'Frequency Penalty',
    description:
      'Reduces repetition by penalizing tokens based on how often they appear. Range: -2.0 to 2.0. Positive values discourage repetition.',
    example: 'Use 0.3-0.7 to reduce repetitive phrasing in longer responses',
    learnMoreUrl: 'https://platform.openai.com/docs/api-reference/chat/create#frequency_penalty',
    bestPractice: 'Start with 0.0, increase if you notice repetitive content'
  },

  presencePenalty: {
    title: 'Presence Penalty',
    description:
      'Encourages the model to talk about new topics by penalizing tokens that have already appeared. Range: -2.0 to 2.0.',
    example: 'Use 0.3-0.7 to encourage discussing diverse topics',
    learnMoreUrl: 'https://platform.openai.com/docs/api-reference/chat/create#presence_penalty',
    bestPractice: 'Useful for brainstorming or when you want varied content'
  }
}
