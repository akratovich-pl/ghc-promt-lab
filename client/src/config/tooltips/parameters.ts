import type { TooltipCategory } from './types'

/**
 * Tooltip content for LLM parameters
 * Content sourced from official provider documentation
 */
export const parameterTooltips: TooltipCategory = {
  temperature: {
    title: 'Temperature',
    description:
      'Controls randomness in responses. Lower values make output more focused and deterministic, higher values make it more creative.',
    learnMoreUrl: '/glossary#temperature'
  },

  topP: {
    title: 'Top P (Nucleus Sampling)',
    description:
      'Controls diversity by limiting which tokens the model considers. Lower values make output more focused.',
    learnMoreUrl: '/glossary#top-p'
  },

  maxTokens: {
    title: 'Max Tokens',
    description:
      'Maximum number of tokens the model can generate. Controls output length and affects cost.',
    learnMoreUrl: '/glossary#max-tokens'
  },

  topK: {
    title: 'Top K',
    description:
      'Limits the number of highest probability tokens considered at each step. Lower values make output more focused.',
    learnMoreUrl: '/glossary#top-k'
  },

  frequencyPenalty: {
    title: 'Frequency Penalty',
    description:
      'Reduces repetition by penalizing tokens based on how often they appear. Positive values discourage repetition.',
    learnMoreUrl: '/glossary#frequency-penalty'
  },

  presencePenalty: {
    title: 'Presence Penalty',
    description:
      'Encourages the model to talk about new topics by penalizing tokens that have already appeared.',
    learnMoreUrl: '/glossary#presence-penalty'
  }
}
