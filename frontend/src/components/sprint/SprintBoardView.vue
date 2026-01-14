<template>
  <div>
    <!-- Board Header with Sprint Selector -->
    <BoardHeader
      :board="board"
      :boardId="boardId"
      :projectBoards="projectBoards"
      @switch-board="$emit('switch-board', $event)"
      @create-board="$emit('create-board')"
      @edit-board="$emit('edit-board')"
      @delete-board="$emit('delete-board')"
      @create-sprint="$emit('create-sprint')"
    >
      <template #sprint-selector>
        <SprintSelector
          :sprints="sprints"
          :selectedSprintId="selectedSprintId"
          @select="$emit('select-sprint', $event)"
        />
      </template>
    </BoardHeader>

    <!-- Sprint Info Bar -->
    <SprintInfoBar
      :sprint="selectedSprint"
      @start-sprint="$emit('start-sprint', $event)"
      @complete-sprint="$emit('complete-sprint', $event)"
      @edit-sprint="$emit('edit-sprint', $event)"
      @delete-sprint="$emit('delete-sprint', $event)"
    />

    <!-- Shared Board Canvas -->
    <BoardCanvas
      :workItems="filteredWorkItems"
      @create-workitem="$emit('create-workitem', $event)"
      @edit-workitem="$emit('edit-workitem', $event)"
      @delete-workitem="$emit('delete-workitem', $event)"
      @update-status="(workItem, newStatus) => $emit('update-status', workItem, newStatus)"
      @return-to-backlog="$emit('return-to-backlog', $event)"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Board } from '@/types/Project'
import type { WorkItem } from '@/types/WorkItem'
import type { Sprint } from '@/types/Sprint'
import BoardHeader from '../board/BoardHeader.vue'
import SprintInfoBar from '../sprint/SprintInfoBar.vue'
import SprintSelector from '../sprint/SprintSelector.vue'
import BoardCanvas from '../board/BoardCanvas.vue'

interface Props {
  board: Board | null
  boardId: number
  projectBoards: Board[]
  workItems: WorkItem[]
  sprints: Sprint[]
  selectedSprintId: number | null
}

const props = defineProps<Props>()

defineEmits<{
  'switch-board': [boardId: number]
  'create-board': []
  'edit-board': []
  'delete-board': []
  'create-sprint': []
  'select-sprint': [sprintId: number | null]
  'start-sprint': [sprintId: number]
  'complete-sprint': [sprintId: number]
  'edit-sprint': [sprint: Sprint]
  'delete-sprint': [sprintId: number]
  'create-workitem': [status: string]
  'edit-workitem': [workItem: WorkItem]
  'delete-workitem': [id: number]
  'update-status': [workItem: WorkItem, newStatus: string]
  'return-to-backlog': [workItem: WorkItem]
}>()

const selectedSprint = computed(() => {
  if (props.selectedSprintId === null) return undefined
  return props.sprints.find(s => s.id === props.selectedSprintId)
})

const filteredWorkItems = computed(() => {
  if (props.selectedSprintId === null) {
    // Show backlog items (items without sprint)
    return props.workItems.filter(item => !item.sprintId)
  }
  // Show items in selected sprint
  return props.workItems.filter(item => item.sprintId === props.selectedSprintId)
})
</script>