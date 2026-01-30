<template>
  <div class="sticky top-0 z-40 bg-white border border-gray-200 rounded-xl shadow-sm p-4 mb-6">
    <div class="flex flex-col gap-4">
      
      <!-- TOP ROW: Title/Controls & Dates/Actions -->
      <div class="flex justify-between items-center gap-4">
        <!-- LEFT: Title, Status, Controls -->
        <div class="flex items-center gap-4 min-w-0">
          <div v-if="sprint" class="flex items-center gap-3 min-w-0">
             <h1 class="text-xl font-bold text-gray-900 tracking-tight truncate">
              {{ sprint.name }}
            </h1>
            <span 
              class="text-xs px-2.5 py-0.5 rounded-full font-medium border flex-shrink-0"
              :class="getSprintStatusClass(sprint.status)"
            >
              {{ sprint.status }}
            </span>
          </div>
          <div v-else class="text-xl font-bold text-gray-400">
            No Sprint Selected
          </div>
          
          <!-- Controls Slot (Toggle Buttons) -->
          <slot name="controls"></slot>
        </div>

        <!-- RIGHT: Dates, Selector, Menu -->
        <div class="flex items-center gap-4 flex-shrink-0">
          <div v-if="sprint" class="flex items-center gap-1.5 text-sm text-gray-500 bg-gray-50 px-3 py-1.5 rounded-md border border-gray-100">
              <CalendarIcon class="w-4 h-4 text-gray-400" />
              <span class="whitespace-nowrap font-medium">{{ formatDateRange(sprint) }}</span>
          </div>

          <!-- Action Slot (Sprint Selector) -->
          <slot name="actions"></slot>

           <!-- Actions Menu -->
          <div v-if="sprint" class="relative" ref="menuRef">
            <button
              @click="menuDropdown.toggle"
              class="p-2 text-gray-400 hover:text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
            >
              <MenuIcon class="w-5 h-5" />
            </button>

            <div
              v-if="menuDropdown.isOpen.value"
              class="absolute right-0 top-full mt-2 w-48 bg-white rounded-lg shadow-xl border border-gray-100 py-1 z-50 origin-top-right transform transition-all"
            >
              <div v-if="sprint.status === 'Planning'" class="px-1 py-1">
                <button
                  @click="handleAction('start')"
                  class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 hover:text-green-700 rounded-md flex items-center gap-2"
                >
                  Start Sprint
                </button>
              </div>
              
              <div v-if="sprint.status === 'Active'" class="px-1 py-1">
                <button
                  @click="handleAction('complete')"
                  class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 hover:text-blue-700 rounded-md flex items-center gap-2"
                >
                  Complete Sprint
                </button>
              </div>

              <div class="h-px bg-gray-100 my-1"></div>

              <div class="px-1 py-1">
                <button
                  @click="handleAction('edit')"
                  class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded-md flex items-center gap-2"
                >
                  <EditIcon class="w-4 h-4" />
                  Edit Details
                </button>
                <button
                  @click="handleAction('delete')"
                  class="w-full text-left px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-md flex items-center gap-2"
                >
                  <DeleteIcon class="w-4 h-4" />
                  Delete Sprint
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- BOTTOM ROW: Goal & Stats -->
      <div v-if="sprint" class="flex justify-between items-center gap-4 pt-3 border-t border-gray-100">
         <!-- Goal -->
         <div class="flex items-start gap-2 text-sm flex-1 min-w-0">
            <span v-if="sprint.goal" class="text-xs font-bold text-blue-600 uppercase tracking-wide flex-shrink-0 mt-0.5">Goal:</span>
            <p v-if="sprint.goal" class="text-gray-700 font-medium truncate max-w-2xl" :title="sprint.goal">
              {{ sprint.goal }}
            </p>
            <span v-else class="text-gray-400 italic">No goal set</span>
         </div>

         <!-- Stats -->
         <div class="flex items-center gap-6 text-sm text-gray-500 flex-shrink-0">
            <!-- Days Remaining -->
            <div v-if="sprint.status === 'Active'" class="flex items-center gap-1.5" :class="sprint.daysRemaining < 0 ? 'text-red-600' : ''">
              <ClockIcon class="w-4 h-4" />
              <span class="font-medium whitespace-nowrap">
                {{ sprint.daysRemaining < 0 
                  ? `Ended ${Math.abs(sprint.daysRemaining)} days ago` 
                  : `${sprint.daysRemaining} days left` 
                }}
              </span>
            </div>

            <!-- Items -->
            <div class="flex items-baseline gap-1">
                <span class="font-bold text-gray-900">{{ sprint.completedWorkItems }}</span>
                <span class="text-gray-400">/</span>
                <span class="font-bold text-gray-900">{{ sprint.totalWorkItems }}</span>
                <span class="text-xs text-gray-400 uppercase ml-0.5">Items</span>
            </div>

            <!-- Progress -->
            <div class="flex items-center gap-2">
              <div class="w-20 h-1.5 bg-gray-100 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-blue-500 rounded-full transition-all duration-500"
                  :style="{ width: `${sprint.progressPercentage}%` }"
                ></div>
              </div>
              <span class="font-medium text-gray-700 text-xs w-8 text-right">{{ Math.round(sprint.progressPercentage) }}%</span>
            </div>
         </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Sprint } from '@/types/Sprint'
import { formatDateRange } from '@/utils/dates'
import { getSprintStatusClass } from '@/utils/badges'
import { MenuIcon, EditIcon, DeleteIcon, CalendarIcon, ClockIcon } from '@/components/icons'
import { useDropdown } from '@/composables/useDropdown'
import { useClickOutside } from '@/composables/useClickOutside'

interface Props {
  sprint: Sprint | undefined
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'start-sprint': [sprintId: number]
  'complete-sprint': [sprintId: number]
  'edit-sprint': [sprint: Sprint]
  'delete-sprint': [sprintId: number]
}>()

const menuRef = ref<HTMLElement | null>(null)
const menuDropdown = useDropdown()

useClickOutside(menuRef, menuDropdown.close)

const handleAction = (action: 'start' | 'complete' | 'edit' | 'delete') => {
  menuDropdown.close()
  if (!props.sprint) return

  switch (action) {
    case 'start':
      emit('start-sprint', props.sprint.id)
      break
    case 'complete':
      emit('complete-sprint', props.sprint.id)
      break
    case 'edit':
      emit('edit-sprint', props.sprint)
      break
    case 'delete':
      emit('delete-sprint', props.sprint.id)
      break
  }
}
</script>