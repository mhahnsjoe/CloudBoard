<template>
  <div class="grid gap-6" :style="{ gridTemplateColumns: `repeat(${orderedColumns.length}, minmax(0, 1fr))` }">
    <div
      v-for="column in orderedColumns"
      :key="column.id"
      class="bg-gray-100 rounded-lg p-4"
    >
      <!-- Column Header -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-semibold text-gray-800 flex items-center gap-2">
          <span :class="getStatusIconClass(column.name, props.columns)">●</span>
          {{ column.name }}
        </h2>
        <div class="flex items-center gap-2">
          <span class="text-sm text-gray-500 bg-white px-2 py-1 rounded-full">
            {{ getWorkItemsByStatus(column.name).length }}
          </span>
        </div>
      </div>

      <!-- New Item Button (Leftmost Column Only) -->
      <div v-if="column.order === 0" class="mb-3">
        <button
          @click="$emit('create-workitem', column.name)"
          class="w-full py-2 px-3 border-2 border-dashed border-gray-300 rounded-lg text-gray-500 hover:border-blue-400 hover:text-blue-600 transition-colors flex items-center justify-center gap-2"
        >
          <PlusIcon className="w-4 h-4" />
          New Item
        </button>
      </div>

      <!-- Drop Zone -->
      <div
        @drop="onDrop($event, column.name)"
        @dragover.prevent
        @dragenter.prevent
        class="min-h-[500px] space-y-3"
      >
        <KanbanCard
          v-for="workItem in getWorkItemsByStatus(column.name)"
          :key="workItem.id"
          :workItem="workItem"
          :columns="props.columns"
          @dragstart="onDragStart($event, workItem)"
          @edit="$emit('edit-workitem', workItem)"
          @delete="$emit('delete-workitem', workItem.id)"
          @return-to-backlog="$emit('return-to-backlog', workItem)"
        />
        <!-- Empty State -->
        <div
          v-if="getWorkItemsByStatus(column.name).length === 0"
          class="flex items-center justify-center h-32 text-gray-400 text-sm"
        >
          Drop WorkItems here
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { WorkItem } from '@/types/WorkItem'
import type { BoardColumn } from '@/types/Project'
import { getStatusIconClass } from '@/utils/badges'
import KanbanCard from '../kanban/KanbanCard.vue'
import { PlusIcon } from '@/components/icons'

interface Props {
  workItems: WorkItem[]
  columns: BoardColumn[]
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

// Compute ordered columns
const orderedColumns = computed(() =>
  [...props.columns].sort((a, b) => a.order - b.order)
)

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