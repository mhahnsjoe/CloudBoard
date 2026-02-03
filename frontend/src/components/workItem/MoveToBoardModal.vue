<template>
  <Teleport to="body">
    <div class="modal-overlay" @click.self="$emit('close')">
      <div class="modal max-w-md">
        <h2 class="text-xl font-bold mb-4">Move to Board</h2>
        
        <p class="text-gray-600 mb-4">
          Select a board to move <strong>"{{ workItem?.title }}"</strong> to:
        </p>

        <!-- Type Validation Warning -->
        <div v-if="!isAllowedType" class="mb-4 p-4 bg-amber-50 border border-amber-200 rounded-lg flex items-start gap-3">
          <svg class="w-5 h-5 text-amber-500 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
          </svg>
          <div class="text-sm text-amber-800">
            <strong>Only PBI and Bug items</strong> can be moved directly to boards. 
            {{ workItem?.type }}s are used for backlog organization.
          </div>
        </div>

        <!-- Board Selection -->
        <div class="space-y-2 max-h-64 overflow-y-auto mb-6">
          <button
            v-for="board in boards"
            :key="board.id"
            @click="selectedBoardId = board.id"
            class="w-full text-left p-3 rounded-lg border transition-all"
            :class="selectedBoardId === board.id 
              ? 'border-blue-500 bg-blue-50' 
              : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'"
          >
            <!-- Board Info -->
            <div class="flex items-center justify-between">
              <div>
                <div class="font-medium text-gray-800">{{ board.name }}</div>
                <div class="text-sm text-gray-500">{{ board.workItems?.length || 0 }} items</div>
              </div>
              <span 
                class="text-xs px-2 py-1 rounded-full"
                :class="getBoardTypeClass(board.type)"
              >
                {{ board.type }}
              </span>
            </div>
          </button>

          <div v-if="boards.length === 0" class="text-center py-8 text-gray-500">
            No boards available. Create a board first.
          </div>
        </div>

        <!-- Sprint Selection (Scrum only) -->
        <div 
          v-if="selectedBoard?.type === 'Scrum'"
          class="mb-6 p-4 bg-gray-50 rounded-lg border border-gray-200"
        >
          <label class="block text-xs font-semibold text-gray-700 mb-1.5 uppercase tracking-wide">Target Sprint</label>
          
          <div class="relative">
            <button
              ref="triggerRef"
              @click="toggleDropdown"
              class="w-full flex items-center justify-between px-3 py-2 text-sm border border-gray-300 rounded-lg hover:bg-white transition-colors bg-white shadow-sm"
              :class="{ 'opacity-50 cursor-not-allowed': loadingSprints }"
              :disabled="loadingSprints"
            >
              <span v-if="selectedSprintId" class="text-gray-900 font-medium truncate">
                {{ sprints.find(s => s.id === selectedSprintId)?.name }}
              </span>
              <span v-else class="text-gray-500">Select a sprint...</span>
              
              <div class="flex items-center gap-2 flex-shrink-0">
                <span v-if="selectedSprintId" class="text-xs px-2 py-0.5 rounded-full bg-blue-100 text-blue-800">
                  {{ sprints.find(s => s.id === selectedSprintId)?.status }}
                </span>
                <svg v-if="loadingSprints" class="animate-spin h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <svg v-else class="w-4 h-4 text-gray-500 transition-transform" :class="{ 'rotate-180': sprintDropdownOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                </svg>
              </div>
            </button>

            <!-- Dropdown Menu (Teleported for Pop-out) -->
            <Teleport to="body">
              <div
                v-if="sprintDropdownOpen"
                ref="dropdownMenuRef"
                class="fixed bg-white border border-gray-200 rounded-lg shadow-xl z-[9999] max-h-60 overflow-y-auto"
                :style="dropdownStyle"
              >
                <div class="p-1 space-y-0.5">
                  <button
                    v-for="sprint in sprints"
                    :key="sprint.id"
                    @click="selectSprint(sprint.id)"
                    class="w-full flex items-center justify-between px-3 py-2 text-sm hover:bg-gray-50 rounded-md transition-colors"
                    :class="{ 'bg-blue-50 text-blue-700': selectedSprintId === sprint.id }"
                  >
                    <span class="font-medium truncate mr-2">{{ sprint.name }}</span>
                    <span 
                      class="text-xs px-2 py-0.5 rounded-full whitespace-nowrap"
                      :class="sprint.status === 'Active' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'"
                    >
                      {{ sprint.status }}
                    </span>
                  </button>
                  
                  <div v-if="sprints.length === 0" class="px-3 py-4 text-center text-sm text-gray-500">
                    No active or planned sprints found.
                  </div>
                </div>
              </div>
            </Teleport>
          </div>
          
          <p v-if="sprints.length === 0 && !loadingSprints" class="text-xs text-orange-600 mt-2 flex items-center gap-1">
            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path></svg>
            Item will be placed in project backlog
          </p>
        </div>

        <!-- Actions -->
        <div class="flex justify-end gap-3">
          <button 
            class="btn btn-light" 
            @click="$emit('close')"
          >
            Cancel
          </button>
          <button 
            class="btn btn-primary" 
            :disabled="!selectedBoardId || loading || !isAllowedType"
            @click="handleMove"
          >
            {{ loading ? 'Moving...' : 'Move to Board' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script lang="ts">
import { defineComponent, ref, computed, type PropType, watch, onMounted, onBeforeUnmount, nextTick, type CSSProperties } from 'vue'
import type { WorkItem } from '@/types/WorkItem'
import type { Board } from '@/types/Project'
import type { Sprint } from '@/types/Sprint'
import { getSprints } from '@/services/api'

export default defineComponent({
  name: 'MoveToBoardModal',
  props: {
    workItem: {
      type: Object as PropType<WorkItem | null>,
      default: null
    },
    boards: {
      type: Array as PropType<Board[]>,
      required: true
    }
  },
  emits: ['close', 'move'],
  setup(props, { emit }) {
    const selectedBoardId = ref<number | null>(null)
    const selectedSprintId = ref<number | null>(null)
    const sprints = ref<Sprint[]>([])
    const loading = ref(false)
    const loadingSprints = ref(false)
    
    // Dropdown state
    const sprintDropdownOpen = ref(false)
    const dropdownMenuRef = ref<HTMLElement | null>(null)
    const triggerRef = ref<HTMLElement | null>(null)
    const dropdownStyle = ref<CSSProperties>({})

    const isAllowedType = computed(() => {
      if (!props.workItem) return false
      return props.workItem.type === 'PBI' || props.workItem.type === 'Bug'
    })
    
    const selectedBoard = computed(() => 
      props.boards.find(b => b.id === selectedBoardId.value)
    )

    const getBoardTypeClass = (type: string) => {
      const classes: Record<string, string> = {
        'Kanban': 'bg-blue-100 text-blue-700',
        'Scrum': 'bg-green-100 text-green-700',
        'Backlog': 'bg-purple-100 text-purple-700'
      }
      return classes[type] || 'bg-gray-100 text-gray-700'
    }

    watch(selectedBoardId, async (newVal) => {
      selectedSprintId.value = null
      sprints.value = []
      sprintDropdownOpen.value = false
      
      if (!newVal) return

      const board = props.boards.find(b => b.id === newVal)
      if (board?.type === 'Scrum') {
        loadingSprints.value = true
        try {
          const res = await getSprints(newVal)
          // Filter: Only Active and Planning sprints
          sprints.value = res.data.filter(s => s.status === 'Active' || s.status === 'Planning')
          
          // Auto-select 'Active' sprint first, then 'Planning'
          const defaultSprint = sprints.value.find(s => s.status === 'Active') || 
                                sprints.value.find(s => s.status === 'Planning')
          if (defaultSprint) {
            selectedSprintId.value = defaultSprint.id
          }
        } catch (error) {
          console.error('Failed to fetch sprints:', error)
        } finally {
          loadingSprints.value = false
        }
      }
    })

    const updateDropdownPosition = () => {
      if (!triggerRef.value) return
      
      const rect = triggerRef.value.getBoundingClientRect()
      // Default to opening downwards
      dropdownStyle.value = {
        top: `${rect.bottom + 4}px`,
        left: `${rect.left}px`,
        width: `${rect.width}px`
      }
    }

    const toggleDropdown = async () => {
      if (loadingSprints.value) return
      sprintDropdownOpen.value = !sprintDropdownOpen.value
      
      if (sprintDropdownOpen.value) {
        await nextTick()
        updateDropdownPosition()
      }
    }

    const selectSprint = (sprintId: number) => {
      selectedSprintId.value = sprintId
      sprintDropdownOpen.value = false
    }

    const handleMove = async () => {
      if (!selectedBoardId.value || !props.workItem) return
      
      loading.value = true
      try {
        emit('move', props.workItem.id, selectedBoardId.value, selectedSprintId.value)
      } finally {
        loading.value = false
      }
    }
    
    // Close dropdown on click outside
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node
      // Check if click is inside menu or trigger
      if (dropdownMenuRef.value?.contains(target)) return
      if (triggerRef.value?.contains(target)) return
      
      sprintDropdownOpen.value = false
    }

    // Handle scroll/resize to update position or close
    const handleWindowEvents = () => {
      if (sprintDropdownOpen.value) {
        updateDropdownPosition()
      }
    }

    onMounted(() => {
      document.addEventListener('click', handleClickOutside)
      window.addEventListener('resize', handleWindowEvents)
      window.addEventListener('scroll', handleWindowEvents, true) // Capture to detect scroll in parents
    })

    onBeforeUnmount(() => {
      document.removeEventListener('click', handleClickOutside)
      window.removeEventListener('resize', handleWindowEvents)
      window.removeEventListener('scroll', handleWindowEvents, true)
    })

    return {
      selectedBoardId,
      selectedBoard,
      selectedSprintId,
      sprints,
      loading,
      loadingSprints,
      sprintDropdownOpen,
      dropdownMenuRef,
      triggerRef,
      dropdownStyle,
      toggleDropdown,
      selectSprint,
      isAllowedType,
      getBoardTypeClass,
      handleMove
    }
  }
})
</script>