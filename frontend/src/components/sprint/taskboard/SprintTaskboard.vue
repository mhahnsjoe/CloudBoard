<template>
  <div class="sprint-taskboard overflow-auto border-x border-b border-gray-200 rounded-b-xl shadow-sm">
    <!-- Loading State -->
    <div v-if="loading" class="flex flex-col items-center justify-center py-20 bg-white">
      <LoadingIcon className="w-10 h-10 text-blue-600 animate-spin" />
      <span class="mt-4 text-gray-600 font-medium">Loading taskboard...</span>
    </div>

    <!-- Taskboard Grid -->
    <div v-else-if="taskboard" class="min-w-fit bg-white">
      <!-- Column Headers -->
      <div class="flex border-b border-gray-200 bg-white sticky top-0 z-30 shadow-sm">
        <div class="w-64 flex-shrink-0 p-4 font-bold text-gray-700 border-r border-gray-200 sticky left-0 bg-white z-40 uppercase tracking-wider text-sm flex items-center">
          Work Items
        </div>
        <div 
          v-for="column in taskboard.columns" 
          :key="column"
          class="flex-1 min-w-[200px] p-4 font-bold text-gray-700 border-r border-gray-200 text-left flex items-center uppercase tracking-wider text-sm bg-white"
        >
          {{ column }}
          <span class="text-xs text-gray-500 ml-2 font-normal bg-gray-100 px-2 py-0.5 rounded-full">
            {{ getColumnTaskCount(column) }}
          </span>
        </div>
      </div>

      <!-- Rows -->
      <div class="taskboard-content flex flex-col gap-2">
        <TaskboardRow
          v-for="row in taskboard.rows"
          :key="row.id"
          :row="row"
          :columns="taskboard.columns"
          @view-details="$emit('view-details', $event)"
          @add-task="$emit('add-task', $event)"
          @delete-task="$emit('delete-task', $event)"
          @update-task-status="handleUpdateTaskStatus"
        />
      </div>

      <!-- Empty State -->
      <div 
        v-if="taskboard.rows.length === 0"
        class="flex flex-col items-center justify-center py-24 text-gray-500 bg-white"
      >
        <ClipboardIcon className="w-16 h-16 mb-4 text-gray-300" />
        <h3 class="text-xl font-semibold text-gray-900 mb-1">No work items in this sprint</h3>
        <p class="text-gray-500">PBIs and standalone Bugs added to the sprint will appear here as rows.</p>
      </div>


    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Taskboard, TaskboardTask } from '@/types/Taskboard'
import TaskboardRow from './TaskboardRow.vue'
import { LoadingIcon, ClipboardIcon } from '@/components/icons'

interface Props {
  taskboard: Taskboard | null
  loading: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'view-details': [id: number]
  'add-task': [parentId: number]
  'delete-task': [taskId: number]
  'update-task-status': [task: TaskboardTask, newStatus: string]
}>()

const overallProgress = computed(() => {
  if (!props.taskboard || props.taskboard.totalHours === 0) return 0
  return (props.taskboard.completedHours / props.taskboard.totalHours) * 100
})

const getColumnTaskCount = (column: string) => {
  if (!props.taskboard) return 0
  return props.taskboard.rows.reduce((count, row) => 
    count + row.tasks.filter(t => t.status === column).length, 0)
}

const handleUpdateTaskStatus = (task: TaskboardTask, newStatus: string) => {
  emit('update-task-status', task, newStatus)
}
</script>

<style scoped>
.sprint-taskboard {
  max-height: calc(100vh - 250px);
}
/* Ensure the row headers stay above cells when scrolling */
.sticky-column {
  position: sticky;
  left: 0;
  z-index: 10;
}
</style>
