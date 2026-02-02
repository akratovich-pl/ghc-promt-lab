<template>
  <div class="model-selection bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-4">
      Model Selection
    </h2>
    
    <!-- Error Display -->
    <div 
      v-if="providerStore.error" 
      class="mb-4 p-4 bg-red-100 dark:bg-red-900/30 border border-red-400 text-red-700 dark:text-red-300 rounded"
    >
      {{ providerStore.error }}
    </div>

    <!-- Loading State -->
    <div v-if="providerStore.loading" class="text-center py-4">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-2 text-gray-600 dark:text-gray-400">Loading providers...</p>
    </div>

    <!-- Provider Selection -->
    <div v-else class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Provider
        </label>
        <select
          v-model="providerStore.selectedProvider"
          @change="onProviderChange"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
                 bg-white dark:bg-gray-700 text-gray-900 dark:text-white
                 focus:ring-2 focus:ring-primary-500 focus:border-transparent"
        >
          <option value="" disabled>Select a provider</option>
          <option 
            v-for="provider in providerStore.providers" 
            :key="provider.name"
            :value="provider.name"
            :disabled="!provider.isAvailable"
          >
            {{ provider.name }} {{ provider.isAvailable ? '' : '(Unavailable)' }}
          </option>
        </select>
      </div>

      <!-- Model Selection -->
      <div v-if="availableModels.length > 0">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Model
        </label>
        <select
          v-model="providerStore.selectedModel"
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
                 bg-white dark:bg-gray-700 text-gray-900 dark:text-white
                 focus:ring-2 focus:ring-primary-500 focus:border-transparent"
        >
          <option value="" disabled>Select a model</option>
          <option 
            v-for="model in availableModels" 
            :key="model"
            :value="model"
          >
            {{ model }}
          </option>
        </select>
      </div>

      <!-- Provider Status -->
      <div 
        v-if="providerStore.selectedProvider" 
        class="mt-4 p-4 bg-green-100 dark:bg-green-900/30 border border-green-400 rounded"
      >
        <p class="text-sm text-green-800 dark:text-green-300">
          <span class="font-semibold">✓ Ready:</span> 
          {{ providerStore.selectedProvider }} - {{ providerStore.selectedModel }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useProviderStore } from '@/stores/providerStore'

const providerStore = useProviderStore()

const availableModels = computed(() => {
  const provider = providerStore.providers.find(
    p => p.name === providerStore.selectedProvider
  )
  return provider?.supportedModels || []
})

const onProviderChange = () => {
  const provider = providerStore.providers.find(
    p => p.name === providerStore.selectedProvider
  )
  if (provider && provider.supportedModels.length > 0) {
    providerStore.setSelectedModel(provider.supportedModels[0])
  }
}

onMounted(() => {
  providerStore.fetchProviders()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>
