<template>
  <div class="taskboard-row flex bg-gray-100 rounded-sm shadow-sm">
    <!-- Row Header (PBI/Bug info) -->
    <div 
      class="w-64 flex-shrink-0 p-3 bg-gray-50 border-r border-gray-200 sticky left-0 z-10 flex flex-col justify-between"
    >
      <div>
        <!-- PBI Type, ID, Title Combined -->
        <div class="flex items-center gap-2 mb-2">
           <WorkItemTypeBadge :type="row.type === 'Bug' ? 'Bug' : 'PBI'" :iconOnly="true" class="flex-shrink-0" />
           <span class="text-xs font-bold text-gray-900 flex-shrink-0">{{ row.id }}</span>
           <button
              @click="$emit('view-details', row.id)"
              class="text-xs font-normal text-gray-900 hover:text-blue-600 truncate text-left min-w-0"
              :title="row.title"
           >
             {{ row.title }}
           </button>
        </div>

        <!-- Assignee -->
        <div class="flex items-center gap-1.5 min-w-0 mt-3">
           <div v-if="row.assignedToName" class="flex items-center gap-1.5 min-w-0" :title="row.assignedToName">
              <div class="w-5 h-5 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center text-[10px] font-medium border border-blue-200 flex-shrink-0">
                {{ getInitials(row.assignedToName) }}
              </div>
              <span class="text-xs text-gray-600 font-medium truncate max-w-[140px]">{{ row.assignedToName }}</span>
           </div>
           <div v-else class="flex items-center gap-1.5 min-w-0" title="Unassigned">
              <div class="w-5 h-5 bg-gray-100 text-gray-400 rounded-full flex items-center justify-center border border-gray-200 flex-shrink-0">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
              </div>
              <span class="text-xs text-gray-400 italic">Unassigned</span>
           </div>
        </div>
      </div>
      

      

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
          @dragstart="onDragStart($event, task)"
          @view-details="$emit('view-details', $event)"
          @delete="$emit('delete-task', $event)"
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
  'view-details': [id: number]
  'add-task': [parentId: number]
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

const getInitials = (name: string) => {
  return name ? name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase() : ''
}
</script>
