<template>
  <div class="grid grid-cols-3 gap-6">
    <div
      v-for="status in STATUSES"
      :key="status"
      class="bg-gray-100 rounded-lg p-4"
    >
      <!-- Column Header -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-semibold text-gray-800 flex items-center gap-2">
          <span :class="getStatusIconClass(status)">●</span>
          {{ status }}
        </h2>
        <div class="flex items-center gap-2">
          <span class="text-sm text-gray-500 bg-white px-2 py-1 rounded-full">
            {{ getWorkItemsByStatus(status).length }}
          </span>
          <button
            @click="$emit('create-workitem', status)"
            class="p-1 hover:bg-white rounded transition-colors text-gray-600 hover:text-blue-600"
            title="Add WorkItem"
          >
            <PlusIcon className="w-4 h-4" />
          </button>
        </div>
      </div>

      <!-- Drop Zone -->
      <div
        @drop="onDrop($event, status)"
        @dragover.prevent
        @dragenter.prevent
        class="min-h-[500px] space-y-3"
      >
        <KanbanCard
          v-for="workItem in getWorkItemsByStatus(status)"
          :key="workItem.id"
          :workItem="workItem"
          @dragstart="onDragStart($event, workItem)"
          @edit="$emit('edit-workitem', workItem)"
          @delete="$emit('delete-workitem', workItem.id)"
          @return-to-backlog="$emit('return-to-backlog', workItem)"
        />
        <!-- Empty State -->
        <div
          v-if="getWorkItemsByStatus(status).length === 0"
          class="flex items-center justify-center h-32 text-gray-400 text-sm"
        >
          Drop WorkItems here
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { WorkItem } from '@/types/WorkItem'
import { STATUSES } from '@/types/Project'
import { getStatusIconClass } from '@/utils/badges'
import KanbanCard from '../kanban/KanbanCard.vue'
import { PlusIcon } from '@/components/icons'

interface Props {
  workItems: WorkItem[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'create-workitem': [status: string]
  'edit-workitem': [workItem: WorkItem]
  'delete-workitem': [id: number]
  'update-status': [workItem: WorkItem, newStatus: string]
  'return-to-backlog': [workItem: WorkItem]
}>()

const draggedWorkItem = ref<WorkItem | null>(null)

const getWorkItemsByStatus = (status: string) => {
  return props.workItems.filter(workItem => workItem.status === status)
}

const onDragStart = (event: DragEvent, workItem: WorkItem) => {
  draggedWorkItem.value = workItem
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
  }
}

const onDrop = async (event: DragEvent, newStatus: string) => {
  event.preventDefault()
  if (draggedWorkItem.value && draggedWorkItem.value.status !== newStatus) {
    emit('update-status', draggedWorkItem.value, newStatus)
  }
  draggedWorkItem.value = null
}
</script>