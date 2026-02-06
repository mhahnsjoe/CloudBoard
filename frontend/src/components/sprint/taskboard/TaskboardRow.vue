<template>
  <div class="taskboard-row flex bg-gray-100 rounded-sm shadow-sm">
    <!-- Row Header (PBI/Bug info) -->
    <!-- Row Header (Work Item Card) -->
    <div 
      class="w-64 flex-shrink-0 p-2 bg-gray-50 border-r border-gray-200 sticky left-0 z-10 flex flex-col pt-3"
    >
       <KanbanCard 
         :workItem="rowAsWorkItem"
         class="mb-2"
         :showExpandable="false"
         @view-details="$emit('view-details', $event)"
         @work-item-updated="$emit('work-item-updated', $event)"
       />
    </div>

    <!-- Task Cells (one per column) -->
    <div 
      v-for="(column, index) in columns" 
      :key="column"
      class="flex-1 min-w-[200px] px-4 py-2 border-r border-gray-100"
      @drop="onDrop($event, column)"
      @dragover.prevent
      @dragenter.prevent="onDragEnter"
      @dragleave="onDragLeave"
    >
      <div class="space-y-2 min-h-[80px]">
        <TaskboardTaskCard
          v-for="task in getTasksByStatus(column)"
          :key="task.id"
          :task="task"
          :boardId="boardId"
          @dragstart="onDragStart($event, task)"
          @view-details="$emit('view-details', $event)"
          @delete="$emit('delete-task', $event)"
          @work-item-updated="$emit('work-item-updated', $event)"
        />
      </div>

      <!-- Add Task Button (First Column Only) -->
      <button
        v-if="index === 0"
        @click="$emit('add-task', row.id)"
        class="w-full mt-2 py-1.5 border border-dashed border-gray-300 rounded text-xs text-gray-400 hover:text-blue-600 hover:border-blue-400 hover:bg-white transition-colors flex items-center justify-center gap-1 group/add"
      >
        <PlusIcon className="w-3 h-3 transition-transform group-hover/add:scale-110" />
        Add Task
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import type { TaskboardRow, TaskboardTask } from '@/types/Taskboard'
import type { TeamMember } from '@/types/Team'
import KanbanCard from '@/components/kanban/KanbanCard.vue'
import TaskboardTaskCard from './TaskboardTaskCard.vue'
import { PlusIcon } from '@/components/icons'

// ...

const rowAsWorkItem = computed(() => {
  return {
    ...props.row,
    boardId: props.boardId, // Ensure boardId is passed for internal logic
    // Add any missing fields if necessary, but row + tasks roughly matches WorkItem shape for display
    // KanbanCard expects children for tasks if expandable, but here we just show the PBI card.
    // If we want the KanbanCard to NOT show children (since they are in the row), we might need to mask them 
    // or the KanbanCard handles it. 
    // Actually, KanbanCard shows children if present. 
    // In TaskboardRow, the 'tasks' are the children.
    // If we pass 'tasks' as 'children', KanbanCard might render them inside the card which is NOT what we want 
    // (we want them in the cells).
    // So we should probably pass children: [] to KanbanCard so it looks like a leaf card (just the PBI).
    children: [] 
  }
})

interface Props {
  row: TaskboardRow
  columns: string[]
  boardId: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'view-details': [id: number]
  'add-task': [parentId: number]
  'delete-task': [taskId: number]
  'update-task-status': [task: TaskboardTask, newStatus: string]
  'work-item-updated': [updatedItem: any]
}>()

// Stores and ref for project members might still be needed if other parts use it, 
// but currently only updateRowAssignee used them. 
// TaskboardTaskCard handles its own.
// So we can remove them.

const getTasksByStatus = (status: string) => {
  return props.row.tasks.filter(t => t.status === status)
}

const onDragStart = (event: DragEvent, task: TaskboardTask) => {
  if (event.dataTransfer) {
    event.dataTransfer.setData('application/json', JSON.stringify(task))
    event.dataTransfer.effectAllowed = 'move'
  }
}

const onDrop = (event: DragEvent, newStatus: string) => {
  const target = event.currentTarget as HTMLElement
  target.classList.remove('bg-blue-50/50')
  
  const data = event.dataTransfer?.getData('application/json')
  if (data) {
    const task = JSON.parse(data) as TaskboardTask
    // Only allow drop if task belongs to this row
    if (task.parentId === props.row.id) {
      emit('update-task-status', task, newStatus)
    }
  }
}

const onDragEnter = (event: DragEvent) => {
  const target = event.currentTarget as HTMLElement
  target.classList.add('bg-blue-50/50')
}

const onDragLeave = (event: DragEvent) => {
  const target = event.currentTarget as HTMLElement
  target.classList.remove('bg-blue-50/50')
}

</script>
