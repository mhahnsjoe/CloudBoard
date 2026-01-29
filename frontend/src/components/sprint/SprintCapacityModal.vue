<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" @click.self="$emit('close')">
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
        <h2 class="text-xl font-bold text-gray-900 mb-4">
          Set Sprint Capacity
        </h2>
        
        <div v-if="sprint" class="mb-4 text-sm text-gray-600">
          <p><strong>{{ sprint.name }}</strong></p>
          <p>{{ formatDateRange(sprint) }}</p>
        </div>

        <div class="mb-6">
          <label class="block text-sm font-medium text-gray-700 mb-1">
            Total Capacity (hours)
          </label>
          <input
            v-model.number="capacity"
            type="number"
            min="0"
            step="0.5"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="e.g., 80"
          />
          <p class="text-xs text-gray-500 mt-1">
            Typical sprint: 40h/person × team size
          </p>
        </div>

        <div class="flex justify-end gap-3">
          <button
            @click="$emit('close')"
            class="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            @click="save"
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Save
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Sprint } from '@/types/Sprint'
import { formatDateRange } from '@/utils/dates'

interface Props {
  sprint: Sprint | null | undefined
  currentCapacity: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  save: [capacity: number]
}>()

const capacity = ref(props.currentCapacity)

const save = () => {
  emit('save', capacity.value)
}
</script>
