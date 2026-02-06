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
      <div class="flex items-center gap-1.5 min-w-0" @click.stop>
         <AssigneeSelector
           :modelValue="task.assignedToId || null"
           :members="projectTeamMembers"
           @update:modelValue="updateAssignee"
         />
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
import { ref, onMounted } from 'vue'
import type { TaskboardTask } from '@/types/Taskboard'
import type { TeamMember } from '@/types/Team'
import AssigneeSelector from '@/components/workItem/AssigneeSelector.vue'
import { useTeamsStore } from '@/stores/teams'
import { useWorkItemStore } from '@/stores/workItemsStore'


interface Props {
  task: TaskboardTask
  boardId: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  dragstart: [event: DragEvent]
  'view-details': [taskId: number]
  'work-item-updated': [updatedItem: any] // Using any here to match KanbanCard usage flexibility, or define a stricter type
}>()

const teamsStore = useTeamsStore()
const workItemStore = useWorkItemStore()
const projectTeamMembers = ref<TeamMember[]>([])

onMounted(async () => {
   if (teamsStore.currentTeam) {
      projectTeamMembers.value = teamsStore.currentTeam.members
   } else {
      projectTeamMembers.value = await teamsStore.getProjectTeamMembers()
   }
})

const updateAssignee = async (newAssigneeId: number | null) => {
  try {
     await workItemStore.assignWorkItem(props.boardId, props.task.id, newAssigneeId)
     
     const member = projectTeamMembers.value.find(m => m.userId === newAssigneeId)
     
     // Emit updated item structure expected by parent
     const updatedItem = { 
        ...props.task, 
        assignedToId: newAssigneeId,
        assignedToName: member ? member.name : undefined
     }
     emit('work-item-updated', updatedItem)
     
  } catch (e) {
     console.error('Failed to assign', e)
  }
}

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
