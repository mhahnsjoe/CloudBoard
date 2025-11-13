<template>
  <div class="relative">
    <button
      @click="isOpen = !isOpen"
      class="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
    >
      <svg class="w-5 h-5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>
      <span class="font-medium">{{ selectedLabel }}</span>
      <svg class="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <!-- Dropdown -->
    <div
      v-if="isOpen"
      class="absolute top-full mt-2 w-64 bg-white border border-gray-300 rounded-md shadow-lg z-50"
    >
      <!-- Backlog Option -->
      <button
        @click="selectOption(null)"
        class="w-full text-left px-4 py-2 hover:bg-gray-50 transition-colors border-b border-gray-200"
        :class="{ 'bg-blue-50 font-medium': selectedSprintId === null }"
      >
        <div class="flex items-center gap-2">
          <svg class="w-5 h-5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
          <span>Backlog</span>
        </div>
      </button>

      <!-- Active Sprint -->
      <div v-if="activeSprint" class="border-b border-gray-200">
        <div class="px-4 py-2 text-xs font-semibold text-gray-500 uppercase">Active</div>
        <button
          @click="selectOption(activeSprint.id)"
          class="w-full text-left px-4 py-2 hover:bg-gray-50 transition-colors"
          :class="{ 'bg-blue-50 font-medium': selectedSprintId === activeSprint.id }"
        >
          <div class="flex items-center justify-between">
            <span>{{ activeSprint.name }}</span>
            <span class="text-xs text-green-600 font-medium">Active</span>
          </div>
          <div class="text-xs text-gray-500 mt-1">
            {{ formatDateRange(activeSprint) }}
          </div>
        </button>
      </div>

      <!-- Planning Sprints -->
      <div v-if="planningSprints.length > 0" class="border-b border-gray-200">
        <div class="px-4 py-2 text-xs font-semibold text-gray-500 uppercase">Planning</div>
        <button
          v-for="sprint in planningSprints"
          :key="sprint.id"
          @click="selectOption(sprint.id)"
          class="w-full text-left px-4 py-2 hover:bg-gray-50 transition-colors"
          :class="{ 'bg-blue-50 font-medium': selectedSprintId === sprint.id }"
        >
          <div>{{ sprint.name }}</div>
          <div class="text-xs text-gray-500 mt-1">
            {{ formatDateRange(sprint) }}
          </div>
        </button>
      </div>

      <!-- Completed Sprints -->
      <div v-if="completedSprints.length > 0">
        <div class="px-4 py-2 text-xs font-semibold text-gray-500 uppercase">Completed</div>
        <button
          v-for="sprint in completedSprints.slice(0, 3)"
          :key="sprint.id"
          @click="selectOption(sprint.id)"
          class="w-full text-left px-4 py-2 hover:bg-gray-50 transition-colors"
          :class="{ 'bg-blue-50 font-medium': selectedSprintId === sprint.id }"
        >
          <div class="flex items-center justify-between">
            <span>{{ sprint.name }}</span>
            <span class="text-xs text-gray-500">✓</span>
          </div>
          <div class="text-xs text-gray-500 mt-1">
            {{ formatDateRange(sprint) }}
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Sprint } from '@/types/Sprint'

interface Props {
  sprints: Sprint[]
  selectedSprintId: number | null
}

const props = defineProps<Props>()
const emit = defineEmits<{
  select: [sprintId: number | null]
}>()

const isOpen = ref(false)

const activeSprint = computed(() =>
  props.sprints.find(s => s.status === 1)
)

const planningSprints = computed(() =>
  props.sprints.filter(s => s.status === 0)
)

const completedSprints = computed(() =>
  props.sprints.filter(s => s.status === 2).sort((a, b) => 
    new Date(b.endDate).getTime() - new Date(a.endDate).getTime()
  )
)

const selectedLabel = computed(() => {
  if (props.selectedSprintId === null) {
    return 'Backlog'
  }
  const sprint = props.sprints.find(s => s.id === props.selectedSprintId)
  return sprint?.name || 'Select Sprint'
})

function selectOption(sprintId: number | null) {
  emit('select', sprintId)
  isOpen.value = false
}

function formatDateRange(sprint: Sprint) {
  const start = new Date(sprint.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  const end = new Date(sprint.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  return `${start} - ${end}`
}
</script>