import type { TooltipCategory } from './types'

/**
 * Tooltip content for AI/LLM concepts
 * Explains fundamental terminology for beginners
 */
export const conceptTooltips: TooltipCategory = {
  tokens: {
    title: 'Tokens',
    description:
      'The basic units that AI models process. A token can be a word, part of a word, or punctuation. Both your prompt and the response count toward token usage.',
    example: '"Hello, world!" = 4 tokens. "Artificial Intelligence" = 2 tokens.',
    learnMoreUrl: 'https://platform.openai.com/tokenizer',
    bestPractice:
      'Monitor token usage to control costs. Most providers charge per token for both input and output.'
  },

  contextWindow: {
    title: 'Context Window',
    description:
      'The maximum number of tokens (input + output) a model can process in a single request. Longer context windows allow for more information but may cost more.',
    example: 'GPT-4o: 128K tokens ≈ 96,000 words, Gemini 1.5 Pro: 2M tokens ≈ 1.5M words',
    learnMoreUrl: 'https://platform.openai.com/docs/models',
    bestPractice: 'Choose models with larger context windows for long documents or conversations.'
  },

  prompt: {
    title: 'Prompt',
    description:
      'The input text you provide to the AI model. Clear, specific prompts lead to better responses. Good prompts include context, instructions, and desired format.',
    example: 'Good: "Explain quantum computing to a 10-year-old using simple analogies."',
    learnMoreUrl: 'https://platform.openai.com/docs/guides/prompt-engineering',
    bestPractice: 'Be specific, provide context, and specify the format you want in responses.'
  },

  systemPrompt: {
    title: 'System Prompt',
    description:
      'Special instructions that define how the AI should behave throughout the conversation. Sets the tone, role, and constraints for responses.',
    example: 'System: "You are a helpful coding tutor. Explain concepts clearly and provide examples."',
    learnMoreUrl: 'https://platform.openai.com/docs/guides/text-generation',
    bestPractice: 'Use system prompts to establish consistent behavior and expertise level.'
  },

  streaming: {
    title: 'Streaming Responses',
    description:
      'Receive the AI response in real-time as it\'s generated, rather than waiting for the complete response. Improves user experience for longer outputs.',
    example: 'Text appears word-by-word as the model generates it, like typing',
    bestPractice: 'Enable streaming for better perceived performance with long responses.'
  },

  latency: {
    title: 'Response Latency',
    description:
      'The time it takes for the model to generate a response. Affected by model size, response length, and server load. Smaller/faster models have lower latency.',
    example: 'Fast models (GPT-4o-mini, Gemini Flash): ~1-2s, Larger models: ~3-5s',
    bestPractice: 'Choose faster models for interactive applications, larger models for complex tasks.'
  }
}
