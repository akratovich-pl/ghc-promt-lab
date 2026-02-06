<template>
  <header class="bg-white border-b border-gray-200 shadow-sm">
    <div class="max-w-7xl mx-auto px-4 py-5 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between">
        <!-- Left: Logo and App Name -->
        <div class="flex items-center gap-3">
          <img 
            src="/logo.png" 
            alt="PromptLab Logo" 
            class="w-12 h-12 rounded-lg"
          />
          <div>
            <div class="flex items-center gap-2">
              <router-link to="/prompt-lab">
                <h1 
                  class="text-3xl font-bold transition-all duration-500 cursor-pointer hover:opacity-90"
                  :class="isAnimated 
                    ? 'app-name-gradient animate-gradient' 
                    : 'text-gray-900'"
                >
                  PromptLab
                </h1>
              </router-link>
              <router-link
                to="/about"
                class="text-xs px-2 py-1 bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200 font-semibold transition-colors"
                title="About PromptLab"
              >
                About
              </router-link>
            </div>
            <p class="text-base text-gray-500 italic">
              {{ tagline }}
            </p>
          </div>
        </div>

        <!-- Right: Custom Navigation Slot -->
        <div class="flex items-center gap-4">
          <slot name="navigation"></slot>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useLlmStore } from '@/stores/llmStore'

interface Props {
  tagline?: string
  useAnimation?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  tagline: 'Where AI Prompts Come to Life',
  useAnimation: false
})

const llmStore = useLlmStore()

const isAnimated = computed(() => {
  return props.useAnimation && llmStore.isApiConnected
})
</script>

<style scoped>
.app-name-gradient {
  background: linear-gradient(
    90deg,
    #10b981 0%,
    #3b82f6 50%,
    #10b981 100%
  );
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.animate-gradient {
  animation: gradient-flow 3s ease-in-out infinite;
}

@keyframes gradient-flow {
  0%, 100% {
    background-position: 0% center;
  }
  50% {
    background-position: 100% center;
  }
}
</style>
