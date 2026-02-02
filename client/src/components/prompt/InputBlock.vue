<template>
  <div 
    ref="blockRef"
    class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 transition-all duration-300 hover:shadow-xl"
    :class="{ 'ring-2 ring-primary-500': isActive }"
  >
    <div class="flex items-center justify-between mb-4">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        📝 Input Prompt
      </h3>
      <div class="flex gap-2 text-sm text-gray-600 dark:text-gray-400">
        <span>{{ characterCount }} chars</span>
        <span class="text-gray-400">|</span>
        <span>~{{ estimatedTokens }} tokens</span>
      </div>
    </div>

    <textarea
      v-model="inputText"
      @input="handleInput"
      placeholder="Enter your prompt here..."
      class="w-full h-40 p-4 border border-gray-300 dark:border-gray-600 rounded-lg 
             bg-gray-50 dark:bg-gray-700 text-gray-900 dark:text-white
             focus:ring-2 focus:ring-primary-500 focus:border-transparent
             resize-none transition-colors"
      :disabled="disabled"
    />

    <div class="mt-4 flex justify-between items-center">
      <div class="text-xs text-gray-500 dark:text-gray-400">
        Tip: Be specific and clear for best results
      </div>
      <button
        v-if="showClearButton && inputText"
        @click="clearInput"
        class="px-3 py-1 text-sm text-gray-600 dark:text-gray-400 
               hover:text-red-600 dark:hover:text-red-400 transition-colors"
      >
        Clear
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { gsap } from 'gsap'

export interface InputBlockProps {
  modelValue?: string
  disabled?: boolean
  isActive?: boolean
  showClearButton?: boolean
}

const props = withDefaults(defineProps<InputBlockProps>(), {
  modelValue: '',
  disabled: false,
  isActive: false,
  showClearButton: true
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'input': [value: string]
}>()

const blockRef = ref<HTMLDivElement>()
const inputText = ref(props.modelValue)

// Approximate token estimation (1 token ≈ 4 characters for English)
const characterCount = computed(() => inputText.value.length)
const estimatedTokens = computed(() => Math.ceil(inputText.value.length / 4))

watch(() => props.modelValue, (newValue) => {
  inputText.value = newValue
})

function handleInput() {
  emit('update:modelValue', inputText.value)
  emit('input', inputText.value)
}

function clearInput() {
  inputText.value = ''
  handleInput()
}

onMounted(() => {
  // Entrance animation
  if (blockRef.value) {
    gsap.from(blockRef.value, {
      opacity: 0,
      y: 20,
      duration: 0.6,
      ease: 'power2.out'
    })
  }
})
</script>
