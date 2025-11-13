<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click.self="emit('close')">
    <div class="bg-white rounded-lg shadow-xl w-full max-w-2xl p-6">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold">{{ isEdit ? 'Edit Sprint' : 'Create New Sprint' }}</h2>
        <button @click="emit('close')" class="text-gray-400 hover:text-gray-600">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <!-- Sprint Name -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Sprint Name *</label>
            <input
              v-model="formData.name"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="e.g., Sprint 1"
            />
          </div>

          <!-- Date Range -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Start Date *</label>
              <input
                v-model="formData.startDate"
                type="date"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">End Date *</label>
              <input
                v-model="formData.endDate"
                type="date"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <!-- Sprint Goal -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Sprint Goal (Optional)</label>
            <textarea
              v-model="formData.goal"
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="What do you want to achieve in this sprint?"
            />
          </div>
        </div>

        <!-- Error Message -->
        <div v-if="errorMessage" class="mt-4 p-3 bg-red-50 border border-red-200 rounded-md text-red-700 text-sm">
          {{ errorMessage }}
        </div>

        <!-- Actions -->
        <div class="mt-6 flex justify-end gap-3">
          <button
            type="button"
            @click="emit('close')"
            class="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="loading"
            class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {{ loading ? 'Saving...' : (isEdit ? 'Update Sprint' : 'Create Sprint') }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { Sprint } from '@/types/Sprint'

interface Props {
  sprint?: Sprint
}
const error = ref<string | null>(null);
const props = defineProps<Props>()
const emit = defineEmits<{
  close: []
  save: [data: { name: string; startDate: string; endDate: string; goal?: string }]
}>()

const isEdit = computed(() => !!props.sprint)

const formData = ref({
  name: '',
  startDate: '',
  endDate: '',
  goal: ''
})

const loading = ref(false)
const errorMessage = ref('')

// Initialize form data if editing
watch(() => props.sprint, (sprint) => {
  if (sprint) {
    const getDatePart = (dateString: string): string => {
      const parts = dateString.split('T')
      return parts[0] ?? ''
    }
    
    formData.value = {
      name: sprint.name,
      startDate: getDatePart(sprint.startDate),
      endDate: getDatePart(sprint.endDate),
      goal: sprint.goal || ''
    }
  }
}, { immediate: true })

async function handleSubmit() {
    errorMessage.value = ''

    // Validate dates
    if (new Date(formData.value.endDate) <= new Date(formData.value.startDate)) {
    errorMessage.value = 'End date must be after start date'
    return
    }

    loading.value = true
    try 
    {
    emit('save', {
        name: formData.value.name,
        startDate: formData.value.startDate,
        endDate: formData.value.endDate,
        goal: formData.value.goal || undefined
    })
} catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to save sprint'
      error.value = message
      throw e
    } finally {
    loading.value = false
  }
}
</script>