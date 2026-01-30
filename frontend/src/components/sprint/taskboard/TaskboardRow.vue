<template>
  <div class="taskboard-row flex border-b border-gray-200">
    <!-- Row Header (PBI/Bug info) -->
    <div 
      class="w-64 flex-shrink-0 p-3 bg-gray-50 border-r border-gray-200 sticky left-0 z-10"
    >
      <div class="flex items-center gap-2 mb-2">
        <WorkItemTypeBadge :type="row.type" size="sm" />
        <button
          @click="$emit('edit-row', row)"
          class="text-sm font-medium text-gray-900 hover:text-blue-600 truncate text-left"
          :title="row.title"
        >
          {{ row.title }}
        </button>
      </div>
      
      <!-- Progress Bar -->
      <div class="mb-2">
        <div class="flex justify-between text-xs text-gray-500 mb-1">
          <span>{{ row.completedHours }}h / {{ row.totalHours }}h</span>
          <span>{{ Math.round(row.progressPercentage) }}%</span>
        </div>
        <div class="h-1.5 bg-gray-200 rounded-full overflow-hidden">
          <div 
            class="h-full bg-green-500 transition-all"
            :style="{ width: `${row.progressPercentage}%` }"
          />
        </div>
      </div>
      
      <!-- Quick Actions -->
      <div class="flex items-center gap-2">
        <button
          @click="$emit('add-task', row.id)"
          class="text-xs text-blue-600 hover:text-blue-800 flex items-center gap-1"
        >
          <PlusIcon className="w-3 h-3" />
          Add Task
        </button>
      </div>
    </div>

    <!-- Task Cells (one per column) -->
    <div 
      v-for="column in columns" 
      :key="column"
      class="flex-1 min-w-[200px] p-2 border-r border-gray-100"
      :class="{ 'bg-green-50/30': column === 'Done' }"
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
          @dragstart="onDragStart($event, task)"
          @edit="$emit('edit-task', task)"
          @delete="$emit('delete-task', task.id)"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TaskboardRow, TaskboardTask } from '@/types/Taskboard'
import WorkItemTypeBadge from '@/components/workItem/WorkItemTypeBadge.vue'
import TaskboardTaskCard from './TaskboardTaskCard.vue'
import { PlusIcon } from '@/components/icons'

interface Props {
  row: TaskboardRow
  columns: string[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'edit-row': [row: TaskboardRow]
  'add-task': [parentId: number]
  'edit-task': [task: TaskboardTask]
  'delete-task': [taskId: number]
  'update-task-status': [task: TaskboardTask, newStatus: string]
}>()

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
