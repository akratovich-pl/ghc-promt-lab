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
  maxWidth: 450
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
  const base = 'absolute z-50'
  switch (props.position) {
    case 'top':
      return `${base} bottom-full left-1/2 -translate-x-1/2 mb-2`
    case 'bottom':
      return `${base} top-full left-1/2 -translate-x-1/2 mt-2`
    case 'left':
      return `${base} right-full top-1/2 -translate-y-1/2 mr-2`
    case 'right':
      return `${base} left-full top-1/2 -translate-y-1/2 ml-2`
    default:
      return `${base} bottom-full left-1/2 -translate-x-1/2 mb-2`
  }
})

// Arrow classes based on position
const arrowClasses = computed(() => {
  const base = 'absolute w-2 h-2 bg-gray-900 transform rotate-45'
  switch (props.position) {
    case 'top':
      return `${base} top-full left-1/2 -translate-x-1/2 -mt-1`
    case 'bottom':
      return `${base} bottom-full left-1/2 -translate-x-1/2 -mb-1`
    case 'left':
      return `${base} left-full top-1/2 -translate-y-1/2 -ml-1`
    case 'right':
      return `${base} right-full top-1/2 -translate-y-1/2 -mr-1`
    default:
      return `${base} top-full left-1/2 -translate-x-1/2 -mt-1`
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
        :style="{ maxWidth: `${maxWidth}px` }"
        class="pointer-events-none"
        role="tooltip"
      >
        <!-- Arrow -->
        <div :class="arrowClasses"></div>

        <!-- Tooltip content -->
        <div class="bg-gray-900 text-white text-sm rounded-lg shadow-xl p-4">
          <!-- Title -->
          <div class="font-semibold mb-2">{{ content.title }}</div>

          <!-- Description -->
          <div class="text-gray-200 leading-relaxed mb-2">
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
          <div v-if="content.learnMoreUrl" class="mt-3 pt-3 border-t border-gray-700">
            <a
              v-if="isExternalLink(content.learnMoreUrl)"
              :href="content.learnMoreUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-blue-400 hover:text-blue-300 text-xs inline-flex items-center gap-1 pointer-events-auto"
              @click.stop
            >
              📚 Learn more
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
              class="text-blue-400 hover:text-blue-300 text-xs inline-flex items-center gap-1 pointer-events-auto"
              @click.stop
            >
              📚 Learn more →
            </router-link>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>
