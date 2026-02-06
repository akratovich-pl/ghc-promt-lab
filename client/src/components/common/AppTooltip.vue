<script setup lang="ts">
import { ref, computed } from 'vue'
import type { TooltipContent } from '@/config/tooltips'

interface Props {
  /** Tooltip content to display */
  content?: TooltipContent
  /** Position of the tooltip (default: top) */
  position?: 'top' | 'bottom' | 'left' | 'right'
  /** Maximum width of tooltip in pixels */
  maxWidth?: number
}

const props = withDefaults(defineProps<Props>(), {
  position: 'top',
  maxWidth: 320
})

const isVisible = ref(false)
const tooltipRef = ref<HTMLElement>()

// Show/hide with slight delay for better UX
let showTimeout: ReturnType<typeof setTimeout>
let hideTimeout: ReturnType<typeof setTimeout>

const show = () => {
  clearTimeout(hideTimeout)
  showTimeout = setTimeout(() => {
    isVisible.value = true
  }, 200)
}

const hide = () => {
  clearTimeout(showTimeout)
  hideTimeout = setTimeout(() => {
    isVisible.value = false
  }, 100)
}

// Check if URL is external
const isExternalLink = (url: string) => {
  return url.startsWith('http://') || url.startsWith('https://')
}

// Position classes based on prop
const positionClasses = computed(() => {
  const base = 'absolute z-[9999]'
  switch (props.position) {
    case 'top':
      return `${base} bottom-full left-0 mb-2`
    case 'bottom':
      return `${base} top-full left-0 mt-2`
    case 'left':
      return `${base} right-full top-0 mr-2`
    case 'right':
      return `${base} left-full top-0 ml-2`
    default:
      return `${base} bottom-full left-0 mb-2`
  }
})

// Arrow classes based on position
const arrowClasses = computed(() => {
  const base = 'absolute w-3 h-3 bg-gray-800 transform rotate-45'
  switch (props.position) {
    case 'top':
      return `${base} top-full left-4 -mt-1.5`
    case 'bottom':
      return `${base} bottom-full left-4 -mb-1.5`
    case 'left':
      return `${base} left-full top-4 -ml-1.5`
    case 'right':
      return `${base} right-full top-4 -mr-1.5`
    default:
      return `${base} top-full left-4 -mt-1.5`
  }
})
</script>

<template>
  <div class="relative inline-block" @mouseenter="show" @mouseleave="hide">
    <!-- Trigger slot (what the user hovers over) -->
    <slot></slot>

    <!-- Tooltip popup -->
    <Transition
      enter-active-class="transition duration-150 ease-out"
      enter-from-class="opacity-0 scale-95"
      enter-to-class="opacity-100 scale-100"
      leave-active-class="transition duration-100 ease-in"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
    >
      <div
        v-if="isVisible && content"
        ref="tooltipRef"
        :class="positionClasses"
        :style="{ maxWidth: `${maxWidth}px`, minWidth: '200px' }"
        class="pointer-events-auto"
        role="tooltip"
      >
        <!-- Arrow -->
        <div :class="arrowClasses"></div>

        <!-- Tooltip content -->
        <div class="bg-gray-800 text-white text-sm rounded-xl shadow-2xl p-4 border border-gray-700">
          <!-- Title -->
          <div class="font-semibold mb-2 text-base">{{ content.title }}</div>

          <!-- Description -->
          <div class="text-gray-100 leading-relaxed">
            {{ content.description }}
          </div>

          <!-- Example (if provided) -->
          <div v-if="content.example" class="mt-3 pt-3 border-t border-gray-700">
            <div class="text-xs text-gray-400 mb-1">Example:</div>
            <div class="text-gray-300 italic">{{ content.example }}</div>
          </div>

          <!-- Best Practice (if provided) -->
          <div v-if="content.bestPractice" class="mt-3 pt-3 border-t border-gray-700">
            <div class="text-xs text-gray-400 mb-1">💡 Best Practice:</div>
            <div class="text-gray-300">{{ content.bestPractice }}</div>
          </div>

          <!-- Learn More Link (if provided) -->
          <div v-if="content.learnMoreUrl" class="mt-3 pt-3 border-t border-gray-600">
            <a
              v-if="isExternalLink(content.learnMoreUrl)"
              :href="content.learnMoreUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-blue-300 hover:text-blue-200 text-xs inline-flex items-center gap-1.5 font-medium transition-colors"
              @click.stop
            >
              <span>📖</span>
              <span>Learn more</span>
              <svg
                class="w-3 h-3"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                />
              </svg>
            </a>
            <router-link
              v-else
              :to="content.learnMoreUrl"
              class="text-blue-300 hover:text-blue-200 text-xs inline-flex items-center gap-1.5 font-medium transition-colors"
              @click.stop
            >
              <span>📖</span>
              <span>Learn more</span>
              <span>→</span>
            </router-link>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>
