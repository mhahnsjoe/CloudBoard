<template>
  <div class="sprint-taskboard overflow-auto border border-gray-200 rounded-lg shadow-sm">
    <!-- Loading State -->
    <div v-if="loading" class="flex flex-col items-center justify-center py-20 bg-white">
      <LoadingIcon className="w-10 h-10 text-blue-600 animate-spin" />
      <span class="mt-4 text-gray-600 font-medium">Loading taskboard...</span>
    </div>

    <!-- Taskboard Grid -->
    <div v-else-if="taskboard" class="min-w-fit bg-white">
      <!-- Column Headers -->
      <div class="flex border-b-2 border-gray-300 bg-gray-50 sticky top-0 z-30">
        <div class="w-64 flex-shrink-0 p-4 font-bold text-gray-700 border-r border-gray-200 sticky left-0 bg-gray-50 z-40">
          Work Items
        </div>
        <div 
          v-for="column in taskboard.columns" 
          :key="column"
          class="flex-1 min-w-[200px] p-4 font-bold text-gray-700 border-r border-gray-200 text-center uppercase tracking-wider text-sm"
          :class="{ 'bg-green-100/50': column === 'Done' }"
        >
          {{ column }}
          <span class="text-xs text-gray-500 ml-1 font-normal">
            ({{ getColumnTaskCount(column) }})
          </span>
        </div>
      </div>

      <!-- Rows -->
      <div class="taskboard-content">
        <TaskboardRow
          v-for="row in taskboard.rows"
          :key="row.id"
          :row="row"
          :columns="taskboard.columns"
          @edit-row="$emit('edit-workitem', $event)"
          @add-task="$emit('add-task', $event)"
          @edit-task="$emit('edit-task', $event)"
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

      <!-- Summary Footer -->
      <div 
        v-if="taskboard.rows.length > 0"
        class="flex border-t-2 border-gray-300 bg-gray-100 sticky bottom-0 z-30"
      >
        <div class="w-64 flex-shrink-0 p-4 font-bold text-gray-800 border-r border-gray-200 sticky left-0 bg-gray-100">
          <div class="flex justify-between items-center">
            <span>TOTAL:</span>
            <span class="text-blue-700">{{ taskboard.totalTasks }} TASKS</span>
          </div>
          <div class="text-xs text-gray-500 mt-1 font-normal">
            {{ taskboard.completedTasks }} COMPLETED
          </div>
        </div>
        <div class="flex-1 p-4 flex items-center gap-6">
          <div class="flex items-center gap-3">
            <span class="text-sm font-bold text-gray-700">TOTAL EFFORT:</span>
            <span class="text-sm font-semibold bg-white px-2 py-1 rounded border border-gray-300">
              {{ taskboard.completedHours }}h / {{ taskboard.totalHours }}h
            </span>
          </div>
          <div class="flex-1 max-w-sm">
            <div class="flex items-center gap-3">
              <div class="flex-1 h-3 bg-gray-300 rounded-full overflow-hidden shadow-inner">
                <div 
                  class="h-full bg-green-500 shadow-sm transition-all duration-500"
                  :style="{ width: `${overallProgress}%` }"
                />
              </div>
              <span class="text-sm font-bold text-gray-700 w-10 text-right">{{ Math.round(overallProgress) }}%</span>
            </div>
          </div>
        </div>
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
  'edit-workitem': [workItem: any]
  'add-task': [parentId: number]
  'edit-task': [task: TaskboardTask]
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
