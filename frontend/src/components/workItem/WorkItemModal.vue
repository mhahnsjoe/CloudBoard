<template>
  <Teleport to="body">
    <div class="modal-overlay" @click.self="$emit('close')">
      <div class="modal max-w-2xl max-h-[90vh] overflow-y-auto">
        <h2 class="text-2xl font-bold mb-6">{{ isEditing ? 'Edit Work Item' : 'Create Work Item' }}</h2>
        
        <div class="space-y-4">
          <!-- Type Selector -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Type</label>
            <div class="grid grid-cols-5 gap-2">
              <button
                v-for="type in availableTypes"
                :key="type"
                @click="handleTypeChange(type)"
                class="p-2 border rounded-lg transition-all text-center"
                :class="form.type === type 
                  ? 'border-blue-500 bg-blue-50 ring-2 ring-blue-200' 
                  : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'"
              >
                <WorkItemTypeBadge :type="type" />
              </button>
            </div>
          </div>

          <!-- Title -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Title *</label>
            <input
              v-model="form.title"
              type="text"
              placeholder="Enter a descriptive title"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              required
            />
          </div>

          <!-- Parent Selector -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Parent Item
              <span class="text-gray-400 font-normal">(optional)</span>
            </label>
            <ParentSelector
              v-model="form.parentId"
              :parents="validParentOptions"
            />
            <p v-if="validParentOptions.length === 0 && form.type !== 'Epic'" class="text-xs text-gray-500 mt-1">
              No valid parents available for this type. Create a higher-level item first.
            </p>
            <p v-if="parentTypeHint" class="text-xs text-blue-600 mt-1">
              {{ parentTypeHint }}
            </p>
          </div>

          <!--TODO DECIDE IF THIS SHOULD BE REMOVED OR USED ANYWHERE-->
          <!-- Board Selector (when multiple boards available) -->
          <!-- <div v-if="showBoardSelector">
            <label class="block text-sm font-medium text-gray-700 mb-1">Board</label>
            <select
              v-model="form.boardId"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
            >
              <option v-for="board in boards" :key="board.id" :value="board.id">
                {{ board.name }} ({{ board.type }})
              </option>
            </select>
          </div> -->

          <!-- Description -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              v-model="form.description"
              placeholder="Add more details about this work item..."
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none min-h-[100px]"
              rows="4"
            />
          </div>

          <!-- Row: Status, Priority -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select 
                v-model="form.status" 
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              >
                <option v-for="status in STATUSES" :key="status">{{ status }}</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select 
                v-model="form.priority" 
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              >
                <option v-for="priority in PRIORITIES" :key="priority">{{ priority }}</option>
              </select>
            </div>
          </div>

          <!-- Row: Due Date, Estimated Hours -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                <CalendarIcon className="w-4 h-4" />
                Due Date
              </label>
              <input
                v-model="form.dueDate"
                type="date"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                <ClockIcon className="w-4 h-4" />
                Estimated Hours
              </label>
              <input
                v-model.number="form.estimatedHours"
                type="number"
                step="0.5"
                min="0"
                placeholder="0.0"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              />
            </div>
          </div>

          <!-- Actual Hours (only when editing) -->
          <div v-if="isEditing" class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                <ClockIcon className="w-4 h-4" />
                Actual Hours
              </label>
              <input
                v-model.number="form.actualHours"
                type="number"
                step="0.5"
                min="0"
                placeholder="0.0"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              />
            </div>
          </div>
        </div>

        <!-- Validation Error -->
        <div v-if="validationError" class="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
          {{ validationError }}
        </div>

        <!-- Actions -->
        <div class="flex justify-end gap-3 mt-6 pt-4 border-t border-gray-200">
          <button 
            class="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            @click="$emit('close')"
          >
            Cancel
          </button>
          <button 
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            @click="handleSave"
            :disabled="!isValid"
          >
            {{ isEditing ? 'Update' : 'Create' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script lang="ts">
import { defineComponent, ref, computed, watch, type PropType } from 'vue'
import type { WorkItem, WorkItemType } from '@/types/WorkItem'
import type { Board } from '@/types/Project'
import { WORKITEM_TYPES } from '@/types/WorkItem'
import { PRIORITIES, STATUSES } from '@/types/Project'
import { HIERARCHY_RULES } from '@/composables/useWorkItemTree'
import WorkItemTypeBadge from './WorkItemTypeBadge.vue'
import ParentSelector from './ParentSelector.vue'
import { ClockIcon, CalendarIcon } from '@/components/icons'

export default defineComponent({
  name: 'WorkItemModal',
  components: {
    WorkItemTypeBadge,
    ParentSelector,
    ClockIcon,
    CalendarIcon
  },
  props: {
    workItem: {
      type: Object as PropType<WorkItem | null>,
      default: null
    },
    boardId: {
      type: Number,
      required: true
    },
    defaultStatus: {
      type: String,
      default: 'To Do'
    },
    defaultType: {
      type: String as PropType<WorkItemType>,
      default: 'Task'
    },
    parentId: {
      type: Number as PropType<number | null>,
      default: null
    },
    availableParents: {
      type: Array as PropType<WorkItem[]>,
      default: () => []
    },
    boards: {
      type: Array as PropType<Board[]>,
      default: () => []
    },
    sprintId: {
      type: Number as PropType<number | null>,
      default: null
    }
  },
  emits: ['close', 'save'],
  setup(props, { emit }) {
    const isEditing = ref(!!props.workItem)
    const validationError = ref<string | null>(null)
    
    const form = ref({
      id: props.workItem?.id || 0,
      title: props.workItem?.title || '',
      type: (props.workItem?.type || props.defaultType) as WorkItemType,
      description: props.workItem?.description || '',
      status: props.workItem?.status || props.defaultStatus || 'To Do',
      priority: props.workItem?.priority || 'Medium',
      dueDate: props.workItem?.dueDate ? props.workItem.dueDate.split('T')[0] : '',
      estimatedHours: props.workItem?.estimatedHours || null,
      actualHours: props.workItem?.actualHours || null,
      boardId: props.workItem?.boardId || props.boardId,
      parentId: props.workItem?.parentId || props.parentId || null,
      sprintId: props.workItem?.sprintId || props.sprintId || null
    })

    // Available types (all types can be created, but parent rules apply)
    const availableTypes = WORKITEM_TYPES

    // Show board selector when multiple boards available and not editing
    const showBoardSelector = computed(() => {
      return props.boards.length > 1 && !isEditing.value
    })

    // Filter valid parents based on selected type
    const validParentOptions = computed(() => {
      const type = form.value.type
      return props.availableParents.filter(parent => {
        // Check if this parent type can have this child type
        const allowedChildren = HIERARCHY_RULES[parent.type] || []
        return allowedChildren.includes(type)
      })
    })

    // Hint about what types can be parents
    const parentTypeHint = computed(() => {
      const type = form.value.type
      const validParentTypes: WorkItemType[] = []
      
      for (const [parentType, allowedChildren] of Object.entries(HIERARCHY_RULES)) {
        if (allowedChildren.includes(type)) {
          validParentTypes.push(parentType as WorkItemType)
        }
      }
      
      if (validParentTypes.length === 0) return null
      return `Can be a child of: ${validParentTypes.join(', ')}`
    })

    // Validation
    const isValid = computed(() => {
      return form.value.title.trim().length > 0
    })

    // Get label for parent dropdown
    const getParentLabel = (parent: WorkItem): string => {
      return `#${parent.id} - ${parent.type}: ${parent.title}`
    }

    // Handle type change - may need to clear parent if invalid
    const handleTypeChange = (newType: WorkItemType) => {
      form.value.type = newType
      
      // Check if current parent is still valid
      if (form.value.parentId) {
        const parent = props.availableParents.find(p => p.id === form.value.parentId)
        if (parent) {
          const allowedChildren = HIERARCHY_RULES[parent.type] || []
          if (!allowedChildren.includes(newType)) {
            form.value.parentId = null
          }
        }
      }
    }

    // Watch for prop changes
    watch(() => props.workItem, (newWorkItem) => {
      if (newWorkItem) {
        isEditing.value = true
        form.value = {
          id: newWorkItem.id,
          title: newWorkItem.title,
          type: newWorkItem.type,
          description: newWorkItem.description || '',
          status: newWorkItem.status,
          priority: newWorkItem.priority,
          dueDate: newWorkItem.dueDate ? newWorkItem.dueDate.split('T')[0] : '',
          estimatedHours: newWorkItem.estimatedHours || null,
          actualHours: newWorkItem.actualHours || null,
          boardId: newWorkItem.boardId,
          parentId: newWorkItem.parentId || null,
          sprintId: newWorkItem.sprintId || null
        }
      } else {
        isEditing.value = false
        form.value.status = props.defaultStatus || 'To Do'
        form.value.type = props.defaultType || 'Task'
        form.value.parentId = props.parentId || null
        form.value.sprintId = props.sprintId || null
      }
    })

    watch(() => props.parentId, (newParentId) => {
      if (!isEditing.value) {
        form.value.parentId = newParentId
      }
    })

    const handleSave = () => {
      validationError.value = null

      if (!form.value.title.trim()) {
        validationError.value = 'Title is required'
        return
      }

      const workItemData: WorkItem = {
        id: form.value.id,
        title: form.value.title,
        description: form.value.description || undefined,
        status: form.value.status,
        priority: form.value.priority,
        type: form.value.type,
        dueDate: form.value.dueDate || undefined,
        estimatedHours: form.value.estimatedHours || undefined,
        actualHours: form.value.actualHours || undefined,
        boardId: form.value.boardId,
        parentId: form.value.parentId || undefined,
        sprintId: form.value.sprintId,
        createdAt: props.workItem?.createdAt || new Date().toISOString()
      }

      emit('save', workItemData)
    }

    return {
      isEditing,
      form,
      validationError,
      isValid,
      availableTypes,
      showBoardSelector,
      validParentOptions,
      parentTypeHint,
      PRIORITIES,
      STATUSES,
      getParentLabel,
      handleTypeChange,
      handleSave
    }
  }
})
</script>

<style scoped>
.modal-overlay {
  @apply fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50;
}

.modal {
  @apply bg-white rounded-xl shadow-2xl p-6 w-full mx-4;
}
</style>