<template>
  <div class="grid border-x border-b border-gray-200 rounded-b-xl shadow-sm overflow-hidden" :style="{ gridTemplateColumns: `repeat(${orderedColumns.length}, minmax(0, 1fr))` }">
    <div
      v-for="column in orderedColumns"
      :key="column.id"
      class="flex flex-col bg-gray-100 h-full border-r border-gray-200 last:border-r-0"
      data-testid="board-column"
    >
      <!-- Column Header -->
      <div class="flex items-center justify-between p-4 bg-white border-b border-gray-200">
        <h2 class="text-sm font-bold text-gray-700 uppercase tracking-wider flex items-center gap-2">
          {{ column.name }}
        </h2>
        <div class="flex items-center gap-2">
          <span 
            class="text-xs font-medium text-gray-600 bg-gray-100 px-2 py-0.5 rounded-full"
            data-testid="column-count-badge"
          >
            {{ getWorkItemsByStatus(column.name).length }}
          </span>
        </div>
      </div>

      <!-- New Item Button (Leftmost Column Only) -->
      <div v-if="column.order === 0 && allowCreate" class="px-3 pt-3" data-testid="new-item-container">
        <button
          @click="$emit('create-workitem', column.name)"
          data-testid="new-item-button"
          class="w-full py-2 px-3 border-2 border-dashed border-gray-300 rounded-lg text-gray-500 hover:border-blue-400 hover:text-blue-600 transition-colors flex items-center justify-center gap-2 bg-white/50"
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
        class="flex-1 p-3 space-y-3 min-h-[500px]"
        data-testid="board-column-dropzone"
      >
        <KanbanCard
          v-for="workItem in getWorkItemsByStatus(column.name)"
          :key="workItem.id"
          :workItem="workItem"
          :columns="props.columns"
          @dragstart="onDragStart($event, workItem)"
          @dragend="onDragEnd"
          @edit="$emit('edit-workitem', $event)"
          @add-child-task="$emit('add-child-task', $event)"
          @view-details="$emit('view-details', $event)"
          @work-item-updated="$emit('work-item-updated', $event)"
        />
        <!-- Empty State -->
        <div
          v-if="getWorkItemsByStatus(column.name).length === 0 && isDragging"
          class="flex items-center justify-center h-32 text-gray-400 text-sm italic"
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

import KanbanCard from '../kanban/KanbanCard.vue'
import { PlusIcon } from '@/components/icons'

interface Props {
  workItems: WorkItem[]
  columns: BoardColumn[]
  allowCreate?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  allowCreate: true
})

const emit = defineEmits<{
  'create-workitem': [status: string]
  'edit-workitem': [workItem: WorkItem]
  'update-status': [workItem: WorkItem, newStatus: string]
  'add-child-task': [parentWorkItem: WorkItem]
  'view-details': [workItemId: number]
  'work-item-updated': [workItem: WorkItem]
}>()

const draggedWorkItem = ref<WorkItem | null>(null)

// Compute ordered columns
const orderedColumns = computed(() =>
  [...props.columns].sort((a, b) => a.order - b.order)
)

// Compute hierarchical items
const hierarchicalItems = computed(() => {
  const items = props.workItems;
  const roots = items.filter(i => !i.parentId);
  return roots.map(root => ({
    ...root,
    children: items.filter(c => c.parentId === root.id)
  }));
});

const getWorkItemsByStatus = (status: string) => {
  return hierarchicalItems.value.filter(workItem => workItem.status === status)
}

const isDragging = ref(false)

const onDragStart = (event: DragEvent, workItem: WorkItem) => {
  isDragging.value = true
  draggedWorkItem.value = workItem
  if (event && event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
  }
}

const onDragEnd = () => {
  isDragging.value = false
  draggedWorkItem.value = null
}

const onDrop = async (event: DragEvent, newStatus: string) => {
  event.preventDefault()
  if (draggedWorkItem.value && draggedWorkItem.value.status !== newStatus) {
    emit('update-status', draggedWorkItem.value, newStatus)
  }
  onDragEnd()
}


</script>