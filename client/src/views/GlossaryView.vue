<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-white to-gray-50">
    <!-- Header -->
    <AppHeader 
      tagline="AI & LLM terminology explained" 
      :use-animation="true"
    >
      <template #navigation>
        <router-link
          to="/about"
          class="inline-flex items-center gap-2 px-4 py-2 bg-white text-gray-700 rounded-lg hover:bg-gray-50 transition-colors duration-200 border border-gray-300 shadow-sm"
        >
          ← Back
        </router-link>
      </template>
    </AppHeader>

    <!-- Main Content -->
    <div class="max-w-4xl mx-auto px-4 py-12 sm:px-6 lg:px-8">
      <!-- Hero Section -->
      <section class="text-center mb-12">
        <h2 class="text-4xl font-bold text-gray-900 mb-4">
          📚 Glossary
        </h2>
        <p class="text-lg text-gray-600 max-w-2xl mx-auto">
          Essential AI and LLM terms explained in simple language
        </p>
      </section>

      <!-- Glossary Terms -->
      <div class="space-y-8">
        <!-- Tokens -->
        <div id="tokens" class="bg-white rounded-xl shadow-md p-6 border border-gray-200 scroll-mt-20">
          <h3 class="text-2xl font-bold text-gray-900 mb-3">
            🎯 Tokens
          </h3>
          <p class="text-gray-700 leading-relaxed mb-4">
            Tokens are the fundamental units of text that AI models process. Rather than working with raw characters or complete words, 
            models break text into tokens using a process called tokenization. A token can represent a complete word, part of a word 
            (like "ing"), punctuation, or even spaces.
          </p>
          <p class="text-gray-700 leading-relaxed mb-4">
            In English, one token typically equals about 4 characters or 0.75 words. For example, "Hello, world!" is broken into 4 tokens: 
            ["Hello", ",", " world", "!"]. Both your input (prompt) and the model's output count toward token usage, and most AI providers 
            charge based on the number of tokens processed.
          </p>
          <div class="mt-4 p-4 bg-blue-50 border-l-4 border-blue-400 rounded">
            <p class="text-sm text-gray-700">
              <strong>💡 Why it matters:</strong> Understanding tokens helps you estimate costs and manage context limits effectively.
            </p>
          </div>
          <div class="mt-4">
            <a 
              href="https://platform.openai.com/tokenizer" 
              target="_blank" 
              rel="noopener noreferrer"
              class="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1"
            >
              OpenAI Tokenizer Tool
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
          </div>
        </div>

        <!-- Temperature -->
        <div id="temperature" class="bg-white rounded-xl shadow-md p-6 border border-gray-200 scroll-mt-20">
          <h3 class="text-2xl font-bold text-gray-900 mb-3">
            🌡️ Temperature
          </h3>
          <p class="text-gray-700 leading-relaxed mb-4">
            Temperature is a parameter that controls the randomness and creativity of AI responses. It ranges from 0 to 2, where lower values 
            make the output more focused and deterministic, while higher values make it more creative and unpredictable.
          </p>
          <div class="space-y-3 mb-4">
            <div class="flex items-start gap-3">
              <span class="font-semibold text-gray-900 min-w-[80px]">0.0 - 0.3:</span>
              <span class="text-gray-700">Highly focused and consistent. Best for factual answers, code generation, and data extraction.</span>
            </div>
            <div class="flex items-start gap-3">
              <span class="font-semibold text-gray-900 min-w-[80px]">0.4 - 0.7:</span>
              <span class="text-gray-700">Balanced creativity and coherence. Good for general conversation and varied responses.</span>
            </div>
            <div class="flex items-start gap-3">
              <span class="font-semibold text-gray-900 min-w-[80px]">0.8 - 2.0:</span>
              <span class="text-gray-700">Highly creative and diverse. Ideal for brainstorming, creative writing, and exploring ideas.</span>
            </div>
          </div>
          <div class="mt-4 p-4 bg-blue-50 border-l-4 border-blue-400 rounded">
            <p class="text-sm text-gray-700">
              <strong>💡 Pro tip:</strong> Start with 0.7 for general use, then adjust based on your needs. Lower for precision, higher for creativity.
            </p>
          </div>
          <div class="mt-4 space-x-4">
            <a 
              href="https://platform.openai.com/docs/guides/text-generation" 
              target="_blank" 
              rel="noopener noreferrer"
              class="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1"
            >
              OpenAI Text Generation Guide
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
            <a 
              href="https://docs.anthropic.com/claude/docs/configuring-gpt-temperature" 
              target="_blank" 
              rel="noopener noreferrer"
              class="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1"
            >
              Anthropic Temperature Guide
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
          </div>
        </div>

        <!-- Prompt -->
        <div id="prompt" class="bg-white rounded-xl shadow-md p-6 border border-gray-200 scroll-mt-20">
          <h3 class="text-2xl font-bold text-gray-900 mb-3">
            💬 Prompt
          </h3>
          <p class="text-gray-700 leading-relaxed mb-4">
            A prompt is the input text you provide to an AI model to get a response. The quality of your prompt directly affects the quality 
            of the output. Effective prompts are clear, specific, and provide necessary context.
          </p>
          <p class="text-gray-700 leading-relaxed mb-4">
            Prompts can range from simple questions ("What is photosynthesis?") to complex instructions with examples, constraints, and 
            desired output formats. The art of crafting effective prompts is called prompt engineering.
          </p>
          <div class="bg-gray-50 rounded-lg p-4 mb-4 space-y-3">
            <div>
              <p class="font-semibold text-gray-900 mb-1">Simple Prompt:</p>
              <p class="text-sm text-gray-700 font-mono bg-white p-2 rounded border border-gray-200">
                "Explain quantum computing"
              </p>
            </div>
            <div>
              <p class="font-semibold text-gray-900 mb-1">Better Prompt:</p>
              <p class="text-sm text-gray-700 font-mono bg-white p-2 rounded border border-gray-200">
                "Explain quantum computing to a 10-year-old using everyday analogies, in 2-3 paragraphs"
              </p>
            </div>
          </div>
          <div class="mt-4 p-4 bg-blue-50 border-l-4 border-blue-400 rounded">
            <p class="text-sm text-gray-700">
              <strong>💡 Best practices:</strong> Be specific, provide context, use examples, and clearly state your desired format or constraints.
            </p>
          </div>
          <div class="mt-4 space-x-4">
            <a 
              href="https://platform.openai.com/docs/guides/prompt-engineering" 
              target="_blank" 
              rel="noopener noreferrer"
              class="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1"
            >
              OpenAI Prompt Engineering Guide
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
            <a 
              href="https://docs.anthropic.com/claude/docs/prompt-engineering" 
              target="_blank" 
              rel="noopener noreferrer"
              class="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1"
            >
              Anthropic Prompt Engineering
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
          </div>
        </div>

        <!-- Context Window -->
        <div id="context-window" class="bg-white rounded-xl shadow-md p-6 border border-gray-200 scroll-mt-20">
          <h3 class="text-2xl font-bold text-gray-900 mb-3">
            🪟 Context Window
          </h3>
          <p class="text-gray-700 leading-relaxed mb-4">
            The context window is the maximum amount of text (measured in tokens) that a model can process at once, including both 
            input and output. Think of it as the model's short-term memory. When you exceed this limit, the model must truncate or 
            forget earlier parts of the conversation.
          </p>
          <div class="bg-gray-50 rounded-lg p-4 mb-4">
            <p class="font-semibold text-gray-900 mb-2">Common Context Windows:</p>
            <ul class="space-y-1 text-sm text-gray-700">
              <li>• GPT-3.5 Turbo: 16,385 tokens (~12,000 words)</li>
              <li>• GPT-4: 8,192 tokens (~6,000 words)</li>
              <li>• GPT-4 Turbo: 128,000 tokens (~96,000 words)</li>
              <li>• Claude 3: 200,000 tokens (~150,000 words)</li>
              <li>• Gemini 1.5 Pro: 1,000,000 tokens (~750,000 words)</li>
            </ul>
          </div>
          <div class="mt-4">
            <a 
              href="https://platform.openai.com/docs/models" 
              target="_blank" 
              rel="noopener noreferrer"
              class="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1"
            >
              OpenAI Model Context Lengths
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
          </div>
        </div>

        <!-- Latency -->
        <div id="latency" class="bg-white rounded-xl shadow-md p-6 border border-gray-200 scroll-mt-20">
          <h3 class="text-2xl font-bold text-gray-900 mb-3">
            ⚡ Latency
          </h3>
          <p class="text-gray-700 leading-relaxed mb-4">
            Latency refers to the time delay between sending a prompt and receiving the first token or complete response from the AI model. 
            It consists of network latency, queue time, processing time, and return time.
          </p>
          <p class="text-gray-700 leading-relaxed mb-4">
            Lower latency is critical for real-time applications like chatbots and voice assistants. Latency depends on model size, 
            prompt length, server load, and geographic distance from the API servers.
          </p>
          <div class="mt-4 p-4 bg-blue-50 border-l-4 border-blue-400 rounded">
            <p class="text-sm text-gray-700">
              <strong>💡 Tip:</strong> Choose faster models (like GPT-3.5 or Claude Haiku) for latency-sensitive applications, 
              or use streaming to improve perceived performance.
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import AppHeader from '@/components/common/AppHeader.vue'

onMounted(() => {
  // Handle hash navigation
  if (window.location.hash) {
    const element = document.querySelector(window.location.hash)
    if (element) {
      setTimeout(() => {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' })
      }, 100)
    }
  }
})
</script>
