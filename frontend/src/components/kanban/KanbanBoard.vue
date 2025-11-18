<template>
  <div>
    <!-- Board Header (no sprint selector) -->
    <BoardHeader
      :board="board"
      :boardId="boardId"
      :projectBoards="projectBoards"
      :hideSprintButton="true"
      @switch-board="$emit('switch-board', $event)"
      @create-board="$emit('create-board')"
      @edit-board="$emit('edit-board')"
      @delete-board="$emit('delete-board')"
    />

    <!-- Shared Board Canvas -->
    <BoardCanvas
        :workItems="workItems"
        @create-workitem="$emit('create-workitem', $event)"
        @edit-workitem="$emit('edit-workitem', $event)"
        @delete-workitem="$emit('delete-workitem', $event)"
        @update-status="(workItem, newStatus) => $emit('update-status', workItem, newStatus)"
    />
  </div>
</template>

<script setup lang="ts">
import type { Board } from '@/types/Project'
import type { WorkItem } from '@/types/WorkItem'
import BoardHeader from '../board/BoardHeader.vue'
import BoardCanvas from '../board/BoardCanvas.vue'

interface Props {
  board: Board | null
  boardId: number
  projectBoards: Board[]
  workItems: WorkItem[]
}

defineProps<Props>()

defineEmits<{
  'switch-board': [boardId: number]
  'create-board': []
  'edit-board': []
  'delete-board': []
  'create-workitem': [status: string]
  'edit-workitem': [workItem: WorkItem]
  'delete-workitem': [id: number]
  'update-status': [workItem: WorkItem, newStatus: string]
}>()
</script>