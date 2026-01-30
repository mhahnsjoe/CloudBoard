<template>
  <div
    class="bg-white border border-gray-200 rounded p-2 cursor-grab active:cursor-grabbing hover:shadow-md transition-shadow group"
    draggable="true"
    @dragstart="$emit('dragstart', $event)"
  >
    <div class="flex items-start justify-between gap-2">
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-1 mb-1">
          <span 
            class="w-2 h-2 rounded-full flex-shrink-0"
            :class="getTypeColor(task.type)"
          />
          <span 
            class="text-xs font-medium text-gray-900 truncate"
            :title="task.title"
          >
            {{ task.title }}
          </span>
        </div>
        
        <div class="flex items-center gap-2 text-xs text-gray-500">
          <span v-if="task.estimatedHours" class="flex items-center gap-0.5" title="Estimated Hours">
            <ClockIcon className="w-3 h-3" />
            {{ task.estimatedHours }}h
          </span>
          <span v-if="task.remainingHours !== null && task.remainingHours !== task.estimatedHours" class="text-xs font-bold text-orange-600" title="Remaining Hours">
            {{ task.remainingHours }}h
          </span>
          <span v-if="task.assignedToName" class="truncate max-w-[80px]" :title="task.assignedToName">
            {{ task.assignedToName }}
          </span>
        </div>
      </div>
      
      <!-- Actions (on hover) -->
      <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <button
          @click.stop="$emit('edit', task)"
          class="p-1 hover:bg-gray-100 rounded"
        >
          <EditIcon className="w-3 h-3 text-gray-500" />
        </button>
        <button
          @click.stop="$emit('delete', task.id)"
          class="p-1 hover:bg-red-50 rounded"
        >
          <DeleteIcon className="w-3 h-3 text-red-500" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TaskboardTask } from '@/types/Taskboard'
import { ClockIcon, EditIcon, DeleteIcon } from '@/components/icons'

interface Props {
  task: TaskboardTask
}

defineProps<Props>()

defineEmits<{
  dragstart: [event: DragEvent]
  edit: [task: TaskboardTask]
  delete: [taskId: number]
}>()

const getTypeColor = (type: string) => {
  switch (type) {
    case 'Task': return 'bg-yellow-400'
    case 'Bug': return 'bg-red-400'
    default: return 'bg-gray-400'
  }
}
</script>
