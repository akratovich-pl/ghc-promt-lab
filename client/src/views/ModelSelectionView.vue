<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 py-12 px-4">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="text-center mb-12">
        <div class="text-6xl mb-4">🤖</div>
        <h1 class="text-4xl font-bold text-gray-900 dark:text-white mb-2">
          Select AI Model
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-300">
          Choose your preferred AI provider and model to get started
        </p>
      </div>

      <!-- Loading State -->
      <div v-if="llmStore.loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        <p class="mt-4 text-gray-600 dark:text-gray-300">Loading providers...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="llmStore.error" class="bg-red-100 dark:bg-red-900 border border-red-400 dark:border-red-600 text-red-700 dark:text-red-200 px-4 py-3 rounded-lg mb-6">
        <p class="font-bold">Error</p>
        <p>{{ llmStore.error }}</p>
        <button 
          @click="llmStore.fetchProviders()"
          class="mt-2 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
        >
          Retry
        </button>
      </div>

      <!-- Provider Selection -->
      <div v-else class="space-y-6">
        <div 
          v-for="provider in llmStore.availableProviders" 
          :key="provider.name"
          class="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 hover:shadow-xl transition-shadow"
        >
          <div class="flex items-start justify-between mb-4">
            <div>
              <h2 class="text-2xl font-semibold text-gray-900 dark:text-white">
                {{ provider.name }}
              </h2>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                {{ provider.supportedModels.length }} model(s) available
              </p>
            </div>
            <div 
              class="px-3 py-1 bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-full text-sm font-medium"
            >
              Available
            </div>
          </div>

          <!-- Model Selection -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <button
              v-for="model in provider.supportedModels"
              :key="model"
              @click="selectModelAndNavigate(provider.name, model)"
              :disabled="selectedProvider === provider.name && selectedModelName === model"
              class="px-4 py-3 border-2 rounded-lg text-left transition-all"
              :class="selectedProvider === provider.name && selectedModelName === model 
                ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-300' 
                : 'border-gray-300 dark:border-gray-600 hover:border-primary-400 dark:hover:border-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-white'"
            >
              <div class="font-medium">{{ model }}</div>
              <div v-if="selectedProvider === provider.name && selectedModelName === model" class="text-xs mt-1">
                ✓ Selected
              </div>
            </button>
          </div>
        </div>

        <!-- No Providers Available -->
        <div 
          v-if="llmStore.availableProviders.length === 0"
          class="bg-yellow-100 dark:bg-yellow-900 border border-yellow-400 dark:border-yellow-600 text-yellow-700 dark:text-yellow-200 px-4 py-3 rounded-lg"
        >
          <p class="font-bold">No Providers Available</p>
          <p>Please configure at least one AI provider to continue.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useLlmStore } from '@/stores/llmStore'

const router = useRouter()
const llmStore = useLlmStore()

const selectedProvider = ref<string | null>(null)
const selectedModelName = ref<string | null>(null)

onMounted(async () => {
  await llmStore.fetchProviders()
  
  // If user already has a selection, pre-populate
  if (llmStore.selectedModel) {
    selectedProvider.value = llmStore.selectedModel.providerName
    selectedModelName.value = llmStore.selectedModel.modelName
  }
})

async function selectModelAndNavigate(providerName: string, modelName: string) {
  selectedProvider.value = providerName
  selectedModelName.value = modelName
  
  await llmStore.selectModel(providerName, modelName)
  
  // Navigate to the main prompt lab
  router.push({ name: 'prompt-lab' })
}
</script>
