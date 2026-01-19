<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-semibold text-gray-900">Board Columns</h3>
        <p class="text-sm text-gray-600 mt-1">
          Customize your board's workflow columns (1-5 columns)
        </p>
      </div>
      <button
        v-if="localColumns.length < 5"
        @click="addColumn"
        class="px-3 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
        </svg>
        Add Column
      </button>
    </div>

    <!-- Column List -->
    <VueDraggable
      v-model="localColumns"
      :animation="150"
      handle=".drag-handle"
      @end="updateOrders"
    >
      <div
        v-for="(column, index) in localColumns"
        :key="column.id || index"
        class="bg-white border border-gray-200 rounded-lg p-4 mb-3 hover:shadow-sm transition-shadow"
      >
        <div class="flex items-start gap-3">
          <!-- Drag Handle -->
          <div class="drag-handle cursor-move pt-2 text-gray-400 hover:text-gray-600">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8h16M4 16h16"/>
            </svg>
          </div>

          <!-- Column Details -->
          <div class="flex-1 space-y-3">
            <div class="grid grid-cols-2 gap-3">
              <!-- Name -->
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">
                  Column Name *
                </label>
                <input
                  v-model="column.name"
                  type="text"
                  placeholder="e.g., In Progress"
                  maxlength="50"
                  class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                  :class="{ 'border-red-300 bg-red-50': !column.name.trim() }"
                />
              </div>

              <!-- Category -->
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">
                  Category *
                </label>
                <select
                  v-model="column.category"
                  class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                >
                  <option value="Proposed">Proposed (To Do)</option>
                  <option value="InProgress">In Progress (Active)</option>
                  <option value="Resolved">Resolved (Done)</option>
                </select>
              </div>
            </div>

            <!-- Order and Badge Preview -->
            <div class="flex items-center justify-between text-xs">
              <span class="text-gray-500">Order: {{ index + 1 }}</span>
              <span
                class="px-2 py-1 rounded"
                :class="getCategoryBadgeClass(column.category)"
              >
                {{ column.category }}
              </span>
            </div>
          </div>

          <!-- Delete Button -->
          <button
            v-if="localColumns.length > 1"
            @click="removeColumn(index)"
            class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
            title="Delete column"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </button>
        </div>
      </div>
    </VueDraggable>

    <!-- Validation Messages -->
    <div v-if="validationError" class="p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
      {{ validationError }}
    </div>

    <!-- Info Message -->
    <div class="p-3 bg-blue-50 border border-blue-200 rounded-lg text-blue-700 text-sm">
      <p class="font-medium mb-1">About Categories:</p>
      <ul class="text-xs space-y-1 ml-4 list-disc">
        <li><strong>Proposed:</strong> Work that hasn't started yet (typically left-most columns)</li>
        <li><strong>InProgress:</strong> Active work being done (middle columns)</li>
        <li><strong>Resolved:</strong> Completed work (typically right-most columns)</li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { VueDraggable } from 'vue-draggable-plus'
import type { BoardColumn } from '@/types/Project'

interface Props {
  columns: BoardColumn[]
}

interface Emits {
  (e: 'update:columns', columns: BoardColumn[]): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Local state for editing
const localColumns = ref<BoardColumn[]>([])
const validationError = ref<string | null>(null)
const isInitialized = ref(false)

// Initialize from props
watch(
  () => props.columns,
  (newColumns) => {
    const shouldUseDefaults = !newColumns || newColumns.length === 0

    if (shouldUseDefaults) {
      // Initialize with default columns if none provided
      const defaults = [
        { id: 0, name: 'To Do', order: 0, category: 'Proposed', createdAt: new Date().toISOString(), boardId: 0 },
        { id: 0, name: 'In Progress', order: 1, category: 'InProgress', createdAt: new Date().toISOString(), boardId: 0 },
        { id: 0, name: 'Done', order: 2, category: 'Resolved', createdAt: new Date().toISOString(), boardId: 0 }
      ]
      localColumns.value = defaults
      // Immediately emit defaults so parent knows about them
      emit('update:columns', defaults)
    } else {
      localColumns.value = newColumns.map(col => ({ ...col }))
    }

    // Mark as initialized after first watch
    setTimeout(() => {
      isInitialized.value = true
    }, 0)
  },
  { immediate: true }
)

// Watch local changes and emit (but only after initialization)
watch(
  localColumns,
  (newColumns) => {
    validateColumns()
    if (isInitialized.value) {
      emit('update:columns', newColumns)
    }
  },
  { deep: true }
)

const addColumn = () => {
  if (localColumns.value.length >= 5) {
    validationError.value = 'Maximum 5 columns allowed'
    return
  }

  const newOrder = localColumns.value.length
  localColumns.value.push({
    id: 0, // Will be assigned by backend
    name: '',
    order: newOrder,
    category: 'InProgress',
    createdAt: new Date().toISOString(),
    boardId: 0
  })
}

const removeColumn = (index: number) => {
  if (localColumns.value.length <= 1) {
    validationError.value = 'At least 1 column is required'
    return
  }
  localColumns.value.splice(index, 1)
  updateOrders()
}

const updateOrders = () => {
  localColumns.value.forEach((col, index) => {
    col.order = index
  })
}

const validateColumns = () => {
  validationError.value = null

  if (localColumns.value.length < 1) {
    validationError.value = 'At least 1 column is required'
    return
  }

  if (localColumns.value.length > 5) {
    validationError.value = 'Maximum 5 columns allowed'
    return
  }

  // Check for empty names
  if (localColumns.value.some(col => !col.name.trim())) {
    validationError.value = 'All columns must have a name'
    return
  }

  // Check for duplicate names
  const names = localColumns.value.map(col => col.name.trim().toLowerCase())
  if (new Set(names).size !== names.length) {
    validationError.value = 'Column names must be unique'
    return
  }
}

const getCategoryBadgeClass = (category: string) => {
  const classes: Record<string, string> = {
    'Proposed': 'bg-gray-100 text-gray-700',
    'InProgress': 'bg-blue-100 text-blue-700',
    'Resolved': 'bg-green-100 text-green-700'
  }
  return classes[category] || 'bg-gray-100 text-gray-700'
}

// Expose validation state to parent
defineExpose({
  isValid: computed(() => validationError.value === null && localColumns.value.every(col => col.name.trim()))
})
</script>
