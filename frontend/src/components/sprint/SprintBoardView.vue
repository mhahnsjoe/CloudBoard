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
        <div class="flex items-center gap-4">
          <SprintSelector
            :sprints="sprints"
            :selectedSprintId="selectedSprintId"
            @select="$emit('select-sprint', $event)"
          />
          
          <nav class="flex items-center gap-2 border-l border-gray-200 pl-4 h-8">
            <router-link
              :to="`/projects/${board?.projectId}/boards/${boardId}/sprint-planning`"
              class="text-sm font-medium text-gray-600 hover:text-blue-600 px-2 py-1 rounded hover:bg-gray-100 transition-all"
              active-class="text-blue-600 bg-blue-50"
            >
              Planning
            </router-link>
            <router-link
              v-if="selectedSprintId"
              :to="`/projects/${board?.projectId}/boards/${boardId}/sprints/${selectedSprintId}/summary`"
              class="text-sm font-medium text-gray-600 hover:text-blue-600 px-2 py-1 rounded hover:bg-gray-100 transition-all"
              active-class="text-blue-600 bg-blue-50"
            >
              Insights
            </router-link>
          </nav>
        </div>
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
      :columns="board?.columns || []"
      @create-workitem="$emit('create-workitem', $event)"
      @edit-workitem="$emit('edit-workitem', $event)"
      @delete-workitem="$emit('delete-workitem', $event)"
      @update-status="(workItem, newStatus) => $emit('update-status', workItem, newStatus)"
      @return-to-backlog="$emit('return-to-backlog', $event)"
      @add-child-task="$emit('add-child-task', $event)"
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
  'add-child-task': [parentWorkItem: WorkItem]
}>()

const selectedSprint = computed(() => {
  if (props.selectedSprintId === null) return undefined
  return props.sprints.find(s => s.id === props.selectedSprintId)
})

const filteredWorkItems = computed(() => {
  let items: WorkItem[]
  
  if (props.selectedSprintId === null) {
    // Show backlog items (items without sprint)
    items = props.workItems.filter(item => !item.sprintId)
  } else {
    // Show items in selected sprint
    items = props.workItems.filter(item => item.sprintId === props.selectedSprintId)
  }
  
  // Group hierarchically - only show root items with children
  const rootItems = items.filter(item => !item.parentId)
  
  return rootItems.map(item => {
    const children = items.filter(c => c.parentId === item.id)
    
    return {
      ...item,
      children: children,
      childCount: children.length
    }
  })
})
</script>