import type { TooltipCategory } from './types'

/**
 * Tooltip content for AI models
 * Content describes capabilities and best use cases
 */
export const modelTooltips: TooltipCategory = {
  'gpt-4o': {
    title: 'GPT-4o (OpenAI)',
    description:
      'Latest flagship model with multimodal capabilities. Excellent for complex reasoning, creative writing, coding, and analysis. High intelligence with fast performance.',
    example: 'Best for: Complex problem-solving, detailed code generation, creative content',
    learnMoreUrl: 'https://platform.openai.com/docs/models/gpt-4o',
    bestPractice: 'Great all-purpose model. Higher cost but best quality for complex tasks.'
  },

  'gpt-4o-mini': {
    title: 'GPT-4o Mini (OpenAI)',
    description:
      'Fast, affordable model for simpler tasks. Great for quick responses, basic coding, and straightforward questions. Good balance of speed and capability.',
    example: 'Best for: Quick answers, simple code, summarization, everyday questions',
    learnMoreUrl: 'https://platform.openai.com/docs/models/gpt-4o-mini',
    bestPractice: 'Excellent for most day-to-day tasks. Much cheaper than GPT-4o.'
  },

  'gpt-3.5-turbo': {
    title: 'GPT-3.5 Turbo (OpenAI)',
    description:
      'Fast and affordable model for straightforward tasks. Good for simple questions, basic coding, and quick responses. Lower capability than GPT-4 models.',
    example: 'Best for: Simple questions, basic coding help, quick drafts',
    learnMoreUrl: 'https://platform.openai.com/docs/models/gpt-3-5-turbo',
    bestPractice: 'Most cost-effective option for simple tasks.'
  },

  'gemini-1.5-pro': {
    title: 'Gemini 1.5 Pro (Google)',
    description:
      'High-capability model with very long context window (up to 2M tokens). Excellent for analyzing large documents, long conversations, and complex tasks.',
    example: 'Best for: Long document analysis, multi-turn conversations, complex reasoning',
    learnMoreUrl: 'https://ai.google.dev/gemini-api/docs/models/gemini',
    bestPractice: 'Use when you need to process very long inputs or maintain extended context.'
  },

  'gemini-1.5-flash': {
    title: 'Gemini 1.5 Flash (Google)',
    description:
      'Fast, efficient model optimized for speed. Good balance of capability and performance. Suitable for most tasks at lower cost.',
    example: 'Best for: Fast responses, everyday tasks, real-time applications',
    learnMoreUrl: 'https://ai.google.dev/gemini-api/docs/models/gemini',
    bestPractice: 'Great for applications requiring quick responses without sacrificing quality.'
  },

  'claude-3-opus': {
    title: 'Claude 3 Opus (Anthropic)',
    description:
      'Most capable Claude model. Excellent for complex analysis, creative writing, and detailed reasoning. Known for nuanced understanding and thoughtful responses.',
    example: 'Best for: Complex analysis, creative writing, detailed explanations',
    learnMoreUrl: 'https://docs.anthropic.com/claude/docs/models-overview',
    bestPractice: 'Top-tier model for tasks requiring deep understanding and nuance.'
  },

  'claude-3-sonnet': {
    title: 'Claude 3 Sonnet (Anthropic)',
    description:
      'Balanced model with strong capabilities at moderate cost. Good for most tasks including coding, analysis, and writing. Reliable and versatile.',
    example: 'Best for: General purpose tasks, coding, writing, analysis',
    learnMoreUrl: 'https://docs.anthropic.com/claude/docs/models-overview',
    bestPractice: 'Excellent all-around model with good cost-to-performance ratio.'
  },

  'claude-3-haiku': {
    title: 'Claude 3 Haiku (Anthropic)',
    description:
      'Fast and affordable model for simpler tasks. Quick responses with good capability. Best for straightforward questions and simple coding.',
    example: 'Best for: Quick answers, simple tasks, high-volume applications',
    learnMoreUrl: 'https://docs.anthropic.com/claude/docs/models-overview',
    bestPractice: 'Most cost-effective Claude model for simple, fast interactions.'
  }
}
