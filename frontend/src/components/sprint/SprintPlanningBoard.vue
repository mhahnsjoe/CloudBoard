<template>
  <div class="h-full flex flex-col">
    <!-- Toolbar -->
    <div class="flex items-center justify-between mb-4">
      <div class="flex items-center gap-4">
        <SprintSelector
          :sprints="planningContext?.sprints || []"
          :selectedSprintId="selectedSprintId"
          @select="selectSprint"
        />
        
        <button
          v-if="selectedSprintId"
          @click="$emit('edit-capacity')"
          class="text-sm text-blue-600 hover:text-blue-800"
        >
          Set Capacity
        </button>
      </div>
      
      <div class="flex items-center gap-2">
        <button
          @click="$emit('create-sprint')"
          class="px-3 py-1.5 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700"
        >
          + New Sprint
        </button>
      </div>
    </div>

    <!-- Two Column Layout -->
    <div class="flex-1 flex gap-6 min-h-0">
      <!-- Backlog Column -->
      <SprintPlanningColumn
        title="Product Backlog"
        :items="backlogItems"
        emptyMessage="No unassigned items"
        @item-drop="handleMoveToBacklog"
        @item-dragstart="setDraggingItem"
      />

      <!-- Sprint Column -->
      <SprintPlanningColumn
        :title="selectedSprint?.name || 'Select a Sprint'"
        :items="sprintItems"
        :capacity="sprintCapacity || undefined"
        :emptyMessage="selectedSprintId ? 'Drop items to add to sprint' : 'Select a sprint first'"
        @item-drop="handleMoveToSprint"
        @item-dragstart="setDraggingItem"
      >
        <template #header-actions>
          <div v-if="selectedSprint" class="flex gap-2">
            <button
              v-if="selectedSprint.status === 'Planning'"
              @click="$emit('start-sprint', selectedSprint.id)"
              class="text-xs px-2 py-1 bg-green-100 text-green-700 rounded hover:bg-green-200"
            >
              Start Sprint
            </button>
          </div>
        </template>
      </SprintPlanningColumn>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { SprintPlanningContext, WorkItemSummary, SprintCapacity } from '@/types/Sprint'
import SprintPlanningColumn from './SprintPlanningColumn.vue'
import SprintSelector from './SprintSelector.vue'

interface Props {
  planningContext: SprintPlanningContext | null
  selectedSprintId: number | null
  sprintCapacity: SprintCapacity | null
  sprintItems: WorkItemSummary[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'select-sprint': [sprintId: number | null]
  'move-to-sprint': [itemIds: number[]]
  'move-to-backlog': [itemIds: number[]]
  'create-sprint': []
  'start-sprint': [sprintId: number]
  'edit-capacity': []
}>()

const draggingItem = ref<WorkItemSummary | null>(null)

const backlogItems = computed(() => props.planningContext?.backlogItems || [])

const selectedSprint = computed(() => 
  props.planningContext?.sprints.find(s => s.id === props.selectedSprintId)
)

const selectSprint = (sprintId: number | null) => {
  emit('select-sprint', sprintId)
}

const setDraggingItem = (item: WorkItemSummary) => {
  draggingItem.value = item
}

const handleMoveToSprint = (item: WorkItemSummary) => {
  if (!props.selectedSprintId) return
  // Include parent and all children
  const itemIds = [item.id, ...item.children.map(c => c.id)]
  emit('move-to-sprint', itemIds)
}

const handleMoveToBacklog = (item: WorkItemSummary) => {
  if (!props.selectedSprintId) return
  // Include parent and all children
  const itemIds = [item.id, ...item.children.map(c => c.id)]
  emit('move-to-backlog', itemIds)
}
</script>
