<template>
  <Teleport to="body">
    <div class="modal-overlay" @click.self="$emit('close')">
      <div class="modal max-w-md">
        <h2 class="text-xl font-bold mb-4">Move to Board</h2>
        
        <p class="text-gray-600 mb-4">
          Select a board to move <strong>"{{ workItem?.title }}"</strong> to:
        </p>

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
            :disabled="!selectedBoardId || loading"
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
import { defineComponent, ref, type PropType } from 'vue'
import type { WorkItem } from '@/types/WorkItem'
import type { Board } from '@/types/Project'

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
    const loading = ref(false)

    const getBoardTypeClass = (type: string) => {
      const classes: Record<string, string> = {
        'Kanban': 'bg-blue-100 text-blue-700',
        'Scrum': 'bg-green-100 text-green-700',
        'Backlog': 'bg-purple-100 text-purple-700'
      }
      return classes[type] || 'bg-gray-100 text-gray-700'
    }

    const handleMove = async () => {
      if (!selectedBoardId.value || !props.workItem) return
      
      loading.value = true
      try {
        emit('move', props.workItem.id, selectedBoardId.value)
      } finally {
        loading.value = false
      }
    }

    return {
      selectedBoardId,
      loading,
      getBoardTypeClass,
      handleMove
    }
  }
})
</script>