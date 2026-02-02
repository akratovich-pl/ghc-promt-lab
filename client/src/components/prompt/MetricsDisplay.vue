<template>
  <div class="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
    <div class="p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
      <div class="text-xs text-blue-600 dark:text-blue-400 mb-1">Input Tokens</div>
      <div class="font-semibold text-blue-900 dark:text-blue-200">
        {{ formatNumber(inputTokens) }}
      </div>
    </div>

    <div class="p-3 bg-green-50 dark:bg-green-900/30 rounded-lg">
      <div class="text-xs text-green-600 dark:text-green-400 mb-1">Output Tokens</div>
      <div class="font-semibold text-green-900 dark:text-green-200">
        {{ formatNumber(outputTokens) }}
      </div>
    </div>

    <div class="p-3 bg-purple-50 dark:bg-purple-900/30 rounded-lg">
      <div class="text-xs text-purple-600 dark:text-purple-400 mb-1">Duration</div>
      <div class="font-semibold text-purple-900 dark:text-purple-200">
        {{ formatDuration(duration) }}
      </div>
    </div>

    <div class="p-3 bg-pink-50 dark:bg-pink-900/30 rounded-lg">
      <div class="text-xs text-pink-600 dark:text-pink-400 mb-1">Cost</div>
      <div class="font-semibold text-pink-900 dark:text-pink-200">
        ${{ formatCost(cost) }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
export interface MetricsDisplayProps {
  inputTokens?: number
  outputTokens?: number
  duration?: number
  cost?: number
}

const props = withDefaults(defineProps<MetricsDisplayProps>(), {
  inputTokens: 0,
  outputTokens: 0,
  duration: 0,
  cost: 0
})

function formatNumber(num: number): string {
  return new Intl.NumberFormat('en-US').format(num)
}

function formatDuration(ms: number): string {
  if (ms < 1000) {
    return `${ms}ms`
  }
  return `${(ms / 1000).toFixed(2)}s`
}

function formatCost(cost: number): string {
  return cost.toFixed(6)
}
</script>
