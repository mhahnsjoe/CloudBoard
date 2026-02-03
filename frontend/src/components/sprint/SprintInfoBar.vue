<template>
  <div class="sticky top-0 z-40 bg-white border-x border-t border-gray-200 rounded-t-xl shadow-sm p-4">
    <div class="flex flex-col gap-4">
      
      <!-- TOP ROW: Title/Controls & Dates/Actions -->
      <div class="flex justify-between items-center gap-4">
        <!-- LEFT: Controls (Tabs) -->
        <div class="flex items-center gap-4 min-w-0 flex-1">
          <!-- Controls Slot (Toggle Buttons) -->
          <div class="flex-shrink-0">
             <slot name="controls"></slot>
          </div>
        </div>

        <!-- RIGHT: Status, Dates, Selector, Menu -->
        <div class="flex items-center gap-3 flex-shrink-0">
          <!-- Goal (Moved here) -->
           <div v-if="sprint?.goal" class="flex items-center gap-2 max-w-md" :title="sprint.goal">
              <span class="text-xs font-bold text-gray-500 tracking-wide">Sprint goal:</span>
              <span class="text-sm text-gray-700 font-medium truncate block">{{ sprint.goal }}</span>
           </div>
           <div v-if="sprint && !sprint.goal" class="text-xs text-gray-400 italic">No goal set</div>

          <!-- Vertical Divider -->
          <div class="w-px h-5 bg-gray-200 mx-1"></div>

          <!-- Status Badge -->
          <div v-if="sprint">
             <span 
              class="text-xs px-2.5 py-1 rounded-full font-medium border"
              :class="getSprintStatusClass(sprint.status)"
            >
              {{ sprint.status }}
            </span>
          </div>

          <!-- Date Range -->
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

      <!-- BOTTOM ROW: Stats Only (Goal removed) -->
      <div v-if="sprint" class="flex justify-end items-center gap-4 pt-3 border-t border-gray-100">
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