<template>
  <Modal
    :show="show"
    :title="isEditing ? 'Edit Board' : 'Create Board'"
    :submitText="isEditing ? 'Update' : 'Create'"
    :submitDisabled="!isValid"
    @close="$emit('close')"
    @submit="$emit('submit', form)"
  >
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Board Name *</label>
        <input
          v-model="form.name"
          type="text"
          placeholder="e.g., Sprint 1, Backlog, Bug Tracker"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
          required
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Board Type</label>
        <select v-model="form.type" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none">
          <option v-for="type in BOARD_TYPES" :key="type">{{ type }}</option>
        </select>
      </div>

      <!-- Column Editor -->
      <div v-if="show" class="border-t border-gray-200 pt-4">
        <ColumnEditor
          ref="columnEditorRef"
          :columns="form.columns || []"
          @update:columns="form.columns = $event"
        />
      </div>
    </div>
  </Modal>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Board, BoardColumn } from '@/types/Project'
import { BOARD_TYPES } from '@/types/Project'
import Modal from '@/components/common/Modal.vue'
import ColumnEditor from './ColumnEditor.vue'

const props = defineProps<{
  show: boolean
  board?: Board | null
}>()

defineEmits<{
  (e: 'close'): void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (e: 'submit', form: Record<string, any>): void
}>()

const form = ref<{ id?: number; name: string; type: string; columns?: BoardColumn[] }>({
  name: "",
  type: "Kanban",
  columns: undefined
})

const columnEditorRef = ref<InstanceType<typeof ColumnEditor> | null>(null)
const isEditing = computed(() => !!props.board)

// Validation
const isValid = computed(() => {
  const hasName = !!form.value.name.trim()
  const columnsValid = columnEditorRef.value ? columnEditorRef.value.isValid : true
  return hasName && columnsValid // Column logic might need refinement if not mounted yet, but this is reactive
})

watch(() => props.show, (newVal) => {
  if (newVal) {
    if (props.board) {
      form.value = {
        id: props.board.id,
        name: props.board.name,
        type: props.board.type,
        columns: props.board.columns ? JSON.parse(JSON.stringify(props.board.columns)) : [] // Deep copy
      }
    } else {
      form.value = {
        name: "",
        type: "Kanban",
        columns: undefined // Will trigger defaults in ColumnEditor
      }
    }
  }
})
</script>
