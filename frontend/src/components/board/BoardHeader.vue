<template>
  <div class="flex justify-between items-center mb-6">
    <div class="flex items-center gap-4">
      <!-- Board Dropdown Selector -->
      <div class="relative" ref="boardDropdownRef">
        <button
          @click="boardDropdown.toggle"
          class="flex items-center gap-2 bg-white px-4 py-2.5 rounded-lg border border-gray-300 hover:border-gray-400 transition-all shadow-sm"
        >
          <FolderIcon className="w-5 h-5 text-gray-600" />
          <span class="text-xl font-bold text-gray-800">{{ board?.name }}</span>
          <svg class="w-5 h-5 text-gray-500 transition-transform duration-200" :class="{ 'rotate-180': boardDropdown.isOpen.value }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </button>

        <!-- Board Dropdown Menu -->
        <div
          v-if="boardDropdown.isOpen.value"
          class="absolute top-full left-0 mt-2 w-72 bg-white rounded-lg shadow-xl border border-gray-200 py-2 z-50"
        >
          <div class="px-3 py-2 text-xs text-gray-500 uppercase tracking-wide border-b border-gray-200">
            Switch Board
          </div>
          
          <!-- Board List -->
          <div class="max-h-64 overflow-y-auto">
            <button
              v-for="b in projectBoards"
              :key="b.id"
              @click="$emit('switch-board', b.id)"
              class="w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors flex items-center justify-between group"
              :class="{ 'bg-blue-50': b.id === boardId }"
            >
              <div class="flex items-center gap-3">
                <ClipboardIcon className="w-4 h-4 text-gray-500" />
                <div>
                  <div class="font-medium text-gray-800" :class="{ 'text-blue-700': b.id === boardId }">
                    {{ b.name }}
                  </div>
                  <div class="text-xs text-gray-500">
                    {{ b.workItems?.length || 0 }} WorkItems
                  </div>
                </div>
              </div>
              <span v-if="b.id === boardId" class="text-blue-600 text-xs font-medium">
                Current
              </span>
            </button>
          </div>

          <!-- Add New Board -->
          <div class="border-t border-gray-200 mt-2">
            <button
              @click="$emit('create-board')"
              class="w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors flex items-center gap-3 text-blue-600 font-medium"
            >
              <PlusIcon className="w-4 h-4" />
              <span>Create New Board</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Board Type Badge -->
      <span :class="getBoardTypeClass(board?.type || '')" class="text-xs px-3 py-1.5 rounded-full font-medium">
        {{ board?.type }}
      </span>

      <!-- Sprint Selector Slot -->
      <slot name="sprint-selector"></slot>
    </div>

    <!-- Board Actions -->
    <div class="flex items-center gap-2">
      <button
        @click="$emit('create-sprint')"
        class="px-4 py-2 bg-green-50 text-green-700 rounded-lg hover:bg-green-100 transition-all flex items-center gap-2"
      >
        <PlusIcon className="w-4 h-4" />
        Create Sprint
      </button>
      <button
        @click="$emit('edit-board')"
        class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-all flex items-center gap-2"
      >
        <EditIcon className="w-4 h-4" />
        Edit Board
      </button>
      <button
        @click="$emit('delete-board')"
        class="px-4 py-2 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition-all flex items-center gap-2"
      >
        <DeleteIcon className="w-4 h-4" />
        Delete Board
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Board } from '@/types/Project'
import { useDropdown } from '@/composables/useDropdown'
import { useClickOutside } from '@/composables/useClickOutside'
import { getBoardTypeClass } from '@/utils/badges'
import {
  PlusIcon,
  EditIcon,
  DeleteIcon,
  FolderIcon,
  ClipboardIcon
} from '@/components/icons'

interface Props {
  board: Board | null
  boardId: number
  projectBoards: Board[]
}

defineProps<Props>()

defineEmits<{
  'switch-board': [boardId: number]
  'create-board': []
  'edit-board': []
  'delete-board': []
  'create-sprint': []
}>()

const boardDropdownRef = ref<HTMLElement | null>(null)
const boardDropdown = useDropdown()

useClickOutside(boardDropdownRef, boardDropdown.close)
</script>

<style scoped>
.rotate-180 {
  transform: rotate(180deg);
}
</style>