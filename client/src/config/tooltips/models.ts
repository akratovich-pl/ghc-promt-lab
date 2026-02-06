import type { TooltipCategory } from './types'

/**
 * Tooltip content for AI models
 * Content describes capabilities and best use cases
 */
export const modelTooltips: TooltipCategory = {
  'gpt-4o': {
    title: 'GPT-4o (OpenAI)',
    description:
      'Latest flagship model with multimodal capabilities. Excellent for complex reasoning, creative writing, and coding.',
    learnMoreUrl: '/glossary#gpt-4o'
  },

  'gpt-4o-mini': {
    title: 'GPT-4o Mini (OpenAI)',
    description:
      'Fast, affordable model for simpler tasks. Great balance of speed and capability for everyday questions.',
    learnMoreUrl: '/glossary#gpt-4o-mini'
  },

  'gpt-3.5-turbo': {
    title: 'GPT-3.5 Turbo (OpenAI)',
    description:
      'Fast and affordable model for straightforward tasks. Good for simple questions and basic coding.',
    learnMoreUrl: '/glossary#gpt-3-5-turbo'
  },

  'gemini-1.5-pro': {
    title: 'Gemini 1.5 Pro (Google)',
    description:
      'High-capability model with very long context window (up to 2M tokens). Excellent for analyzing large documents.',
    learnMoreUrl: '/glossary#gemini-1-5-pro'
  },

  'gemini-1.5-flash': {
    title: 'Gemini 1.5 Flash (Google)',
    description:
      'Fast, efficient model optimized for speed. Good balance of capability and performance at lower cost.',
    learnMoreUrl: '/glossary#gemini-1-5-flash'
  },

  'claude-3-opus': {
    title: 'Claude 3 Opus (Anthropic)',
    description:
      'Most capable Claude model. Excellent for complex analysis, creative writing, and detailed reasoning.',
    learnMoreUrl: '/glossary#claude-3-opus'
  },

  'claude-3-sonnet': {
    title: 'Claude 3 Sonnet (Anthropic)',
    description:
      'Balanced model with strong capabilities at moderate cost. Good for most tasks including coding and analysis.',
    learnMoreUrl: '/glossary#claude-3-sonnet'
  },

  'claude-3-haiku': {
    title: 'Claude 3 Haiku (Anthropic)',
    description:
      'Fast and affordable model for simpler tasks. Quick responses with good capability for straightforward questions.',
    learnMoreUrl: '/glossary#claude-3-haiku'
  }
}
