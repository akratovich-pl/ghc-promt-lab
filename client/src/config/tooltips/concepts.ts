import type { TooltipCategory } from './types'

/**
 * Tooltip content for AI/LLM concepts
 * Explains fundamental terminology for beginners
 */
export const conceptTooltips: TooltipCategory = {
  tokens: {
    title: 'Tokens',
    description:
      'The basic units that AI models process. A token can be a word, part of a word, or punctuation.',
    learnMoreUrl: '/glossary#tokens'
  },

  contextWindow: {
    title: 'Context Window',
    description:
      'The maximum number of tokens (input + output) a model can process in a single request.',
    learnMoreUrl: '/glossary#context-window'
  },

  prompt: {
    title: 'Prompt',
    description:
      'The input text you provide to the AI model. Clear, specific prompts lead to better responses.',
    learnMoreUrl: '/glossary#prompt'
  },

  systemPrompt: {
    title: 'System Prompt',
    description:
      'Special instructions that define how the AI should behave throughout the conversation.',
    learnMoreUrl: '/glossary#system-prompt'
  },

  streaming: {
    title: 'Streaming Responses',
    description:
      'Receive the AI response in real-time as it\'s generated, rather than waiting for the complete response.',
    learnMoreUrl: '/glossary#streaming'
  },

  latency: {
    title: 'Response Latency',
    description:
      'The time it takes for the model to generate a response. Affected by model size, response length, and server load.',
    learnMoreUrl: '/glossary#latency'
  }
}
