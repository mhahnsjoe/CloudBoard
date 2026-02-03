<template>
  <div
    class="bg-white border border-gray-200 border-l-[3px] rounded-r p-2.5 cursor-grab active:cursor-grabbing hover:shadow-md transition-shadow group relative"
    :class="getBorderClass(task.type)"
    draggable="true"
    @dragstart="$emit('dragstart', $event)"
  >
    <!-- Header: Title & Priority -->
    <div class="flex items-center justify-between gap-2 mb-2">
      <div class="flex items-center gap-1.5 min-w-0">
        <span 
          class="w-2 h-2 rounded-full flex-shrink-0"
          :class="getTypeColor(task.type)"
          :title="task.type"
        />
        <span class="text-xs font-bold text-gray-900 flex-shrink-0">{{ task.id }}</span>
        <button 
          @click.stop="$emit('view-details', task.id)"
          class="text-xs font-normal text-gray-900 truncate hover:text-blue-600 leading-tight min-w-0"
          :title="task.title"
        >
          {{ task.title }}
        </button>
      </div>
      
      <span class="text-[9px] font-bold uppercase tracking-wider text-gray-500 bg-gray-50 px-1 py-0.5 rounded border border-gray-100 flex-shrink-0">
        {{ task.priority }}
      </span>
    </div>
    
    <!-- Footer: Assignee & Hours -->
    <div class="flex items-center justify-between pt-2 border-t border-gray-50 mt-1">
      <!-- Assignee -->
      <div class="flex items-center gap-1.5 min-w-0">
         <div v-if="task.assignedToName" class="flex items-center gap-1.5 min-w-0" :title="task.assignedToName">
            <div class="w-4 h-4 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center text-[9px] font-bold border border-blue-200 flex-shrink-0">
              {{ getInitials(task.assignedToName) }}
            </div>
            <span class="text-[10px] text-gray-600 font-medium truncate max-w-[70px]">{{ task.assignedToName }}</span>
         </div>
         <div v-else class="flex items-center gap-1.5 min-w-0" title="Unassigned">
            <div class="w-4 h-4 bg-gray-100 text-gray-400 rounded-full flex items-center justify-center border border-gray-200 flex-shrink-0">
              <svg class="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
            </div>
            <span class="text-[10px] text-gray-400 italic">Unassigned</span>
         </div>
      </div>

      <!-- Hours -->
      <div class="text-[10px] text-gray-500 font-mono flex-shrink-0">
        <span v-if="task.remainingHours !== null" :class="{ 'text-orange-600 font-bold': task.remainingHours > 0 }">
           {{ task.remainingHours }}h
        </span>
        <span v-else-if="task.estimatedHours">
           {{ task.estimatedHours }}h
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TaskboardTask } from '@/types/Taskboard'
import { ClockIcon } from '@/components/icons'

interface Props {
  task: TaskboardTask
}

defineProps<Props>()

const emit = defineEmits<{
  dragstart: [event: DragEvent]
  'view-details': [taskId: number]
}>()

const getTypeColor = (type: string) => {
  switch (type) {
    case 'Task': return 'bg-yellow-400'
    case 'Bug': return 'bg-red-400'
    default: return 'bg-gray-400'
  }
}

const getInitials = (name: string) => {
  return name ? name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase() : ''
}

const getBorderClass = (type: string) => {
  const classes: Record<string, string> = {
    'Task': 'border-l-yellow-400',
    'Bug': 'border-l-red-500',
    'PBI': 'border-l-blue-500', // Should rarely appear on taskboard task card, but possible? PBIs are rows. But maybe sub-pbi?
    'Feature': 'border-l-purple-500'
  };
  return classes[type] || 'border-l-gray-300';
}
</script>
