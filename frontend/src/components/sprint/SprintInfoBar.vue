<template>
  <div v-if="sprint" class="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
    <div class="flex justify-between items-start">
      <div class="flex-1">
        <div class="flex items-center gap-3 mb-2">
          <h3 class="text-lg font-bold text-gray-800">{{ sprint.name }}</h3>
          <span 
            class="text-xs px-2 py-1 rounded-full font-medium"
            :class="getSprintStatusClass(sprint.status)"
          >
            {{ sprint.status }}
          </span>
        </div>
        <p v-if="sprint.goal" class="text-gray-700 text-sm mb-2">{{ sprint.goal }}</p>
        <div class="flex items-center gap-4 text-sm text-gray-600">
          <span>{{ formatDateRange(sprint) }}</span>
          <span>•</span>
          <span>{{ sprint.completedWorkItems }}/{{ sprint.totalWorkItems }} items completed</span>
          <span>•</span>
          <span>{{ Math.round(sprint.progressPercentage) }}% done</span>
          <span v-if="sprint.status === 'Active'">•</span>
          <span v-if="sprint.status === 'Active'" :class="sprint.daysRemaining < 0 ? 'text-red-600 font-medium' : ''">
            {{ sprint.daysRemaining }} days remaining
          </span>
        </div>
      </div>
      <div class="flex gap-2">
        <button
          v-if="sprint.status === 'Planning'"
          @click="$emit('start-sprint', sprint.id)"
          class="px-3 py-1.5 bg-green-600 text-white text-sm rounded-md hover:bg-green-700 transition-colors"
        >
          Start Sprint
        </button>
        <button
          v-if="sprint.status === 'Active'"
          @click="$emit('complete-sprint', sprint.id)"
          class="px-3 py-1.5 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 transition-colors"
        >
          Complete Sprint
        </button>
        <button
          @click="$emit('edit-sprint', sprint)"
          class="px-3 py-1.5 bg-white border border-gray-300 text-gray-700 text-sm rounded-md hover:bg-gray-50 transition-colors"
        >
          Edit
        </button>
        <button
          @click="$emit('delete-sprint', sprint.id)"
          class="px-3 py-1.5 bg-red-50 border border-red-200 text-red-600 text-sm rounded-md hover:bg-red-100 transition-colors"
        >
          Delete
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Sprint } from '@/types/Sprint'
import { formatDateRange } from '@/utils/dates'
import { getSprintStatusClass } from '@/utils/badges'

interface Props {
  sprint: Sprint | undefined
}

defineProps<Props>()

defineEmits<{
  'start-sprint': [sprintId: number]
  'complete-sprint': [sprintId: number]
  'edit-sprint': [sprint: Sprint]
  'delete-sprint': [sprintId: number]
}>()
</script>