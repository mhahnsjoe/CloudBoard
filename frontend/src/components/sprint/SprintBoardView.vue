<template>
  <div>
    <!-- Board Header -->
    <BoardHeader
      :board="board"
      :boardId="boardId"
      :projectBoards="projectBoards"
      @switch-board="$emit('switch-board', $event)"
      @create-board="$emit('create-board')"
      @edit-board="$emit('edit-board')"
      @delete-board="$emit('delete-board')"
      @create-sprint="$emit('create-sprint')"
    />

    <!-- Sprint Info Bar with Selector and Controls -->
    <SprintInfoBar
      :sprint="selectedSprint"
      @start-sprint="$emit('start-sprint', $event)"
      @complete-sprint="$emit('complete-sprint', $event)"
      @edit-sprint="$emit('edit-sprint', $event)"
      @delete-sprint="$emit('delete-sprint', $event)"
    >
      <template #controls>
          <div v-if="selectedSprintId" class="flex items-center bg-gray-100 rounded-lg p-1 border border-gray-200">
             <!-- Taskboard Tab -->
             <button
               @click="toggleView('taskboard')"
               class="px-4 py-1.5 text-xs font-medium rounded-md transition-all"
               :class="viewMode === 'taskboard' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-600 hover:text-gray-900 hover:bg-gray-200/50'"
             >
               Taskboard
             </button>

             <!-- Board (Kanban) Tab -->
             <button
               @click="toggleView('kanban')"
               class="px-4 py-1.5 text-xs font-medium rounded-md transition-all"
               :class="viewMode === 'kanban' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-600 hover:text-gray-900 hover:bg-gray-200/50'"
             >
               Board
             </button>

             <!-- Analytics Tab -->
             <button
               @click="toggleView('analytics')"
               class="px-4 py-1.5 text-xs font-medium rounded-md transition-all"
               :class="viewMode === 'analytics' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-600 hover:text-gray-900 hover:bg-gray-200/50'"
             >
               Analytics
             </button>
          </div>
          
          <!-- Planning Link (Separate, if needed, but user said 'just 3 tabs', so hiding it or moving it. 
               The user specifically said JUST have the three tabs. I will remove Planning from this specific bar. 
               Users can access planning via other means or I'll leave it out as requested.) -->
      </template>
      
      <template #actions>
        <SprintSelector
          :sprints="sprints"
          :selectedSprintId="selectedSprintId"
          @select="$emit('select-sprint', $event)"
          @create="$emit('create-sprint')"
        />
      </template>
    </SprintInfoBar>

    <!-- Shared Board Canvas (Kanban View) -->
    <BoardCanvas
      v-if="viewMode === 'kanban'"
      :workItems="filteredWorkItems"
      :columns="board?.columns || []"
      :allowCreate="selectedSprint?.status === 'Planning'"
      @create-workitem="$emit('create-workitem', $event)"
      @edit-workitem="$emit('edit-workitem', $event)"
      @delete-workitem="$emit('delete-workitem', $event)"
      @update-status="(item, status) => $emit('update-status', item, status)"
      @return-to-backlog="$emit('return-to-backlog', $event)"
      @add-child-task="$emit('add-child-task', $event)"
      @view-details="$emit('view-details', $event)"
    />

    <!-- Taskboard View -->
    <SprintTaskboard
      v-else-if="viewMode === 'taskboard'"
      :taskboard="taskboardData"
      :loading="loadingTaskboard"
      @view-details="$emit('view-details', $event)"
      @add-task="onAddTask"
      @update-task-status="handleUpdateTaskStatus"
    />

    <!-- Analytics View -->
    <SprintAnalytics 
      v-else-if="viewMode === 'analytics' && selectedSprint"
      :sprint="selectedSprint"
      :boardId="boardId"
      @update-sprint="$emit('update-sprint', $event)"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, watch, type PropType } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { Board } from '@/types/Project'
import type { WorkItem } from '@/types/WorkItem'
import type { Sprint } from '@/types/Sprint'
import type { Taskboard, TaskboardTask } from '@/types/Taskboard'
import * as api from '@/services/api'
import BoardHeader from '../board/BoardHeader.vue'
import SprintInfoBar from '../sprint/SprintInfoBar.vue'
import SprintSelector from '../sprint/SprintSelector.vue'
import BoardCanvas from '../board/BoardCanvas.vue'
import SprintTaskboard from './taskboard/SprintTaskboard.vue'
import SprintAnalytics from './SprintAnalytics.vue'

export default defineComponent({
  name: 'SprintBoardView',
  components: {
    BoardHeader,
    SprintInfoBar,
    SprintSelector,
    BoardCanvas,
    SprintTaskboard,
    SprintAnalytics
  },
  props: {
    board: {
      type: Object as PropType<Board | null>,
      required: true
    },
    boardId: {
      type: Number,
      required: true
    },
    projectBoards: {
      type: Array as PropType<Board[]>,
      required: true
    },
    workItems: {
      type: Array as PropType<WorkItem[]>,
      required: true
    },
    sprints: {
      type: Array as PropType<Sprint[]>,
      required: true
    },
    selectedSprintId: {
      type: Number as PropType<number | null>,
      required: true
    }
  },
  emits: [
    'switch-board',
    'create-board',
    'edit-board',
    'delete-board',
    'create-sprint',
    'select-sprint',
    'start-sprint',
    'complete-sprint',
    'edit-sprint',
    'delete-sprint',
    'create-workitem',
    'edit-workitem',
    'delete-workitem',
    'update-status',
    'return-to-backlog',
    'add-child-task',
    'view-details',
    'update-sprint'
  ],
  setup(props, { emit }) {
    const route = useRoute()
    const router = useRouter()

    const selectedSprint = computed(() => {
      if (props.selectedSprintId === null) return undefined
      return props.sprints.find(s => s.id === props.selectedSprintId)
    })

    // We no longer need to filter/group here as BoardDetailView handles sprint filtering
    // and BoardCanvas handles hierarchical grouping.
    const filteredWorkItems = computed(() => props.workItems)

    const getInitialViewMode = (): 'kanban' | 'taskboard' | 'analytics' => {
      const view = route.query.view as string
      if (['kanban', 'taskboard', 'analytics'].includes(view)) {
         return view as 'kanban' | 'taskboard' | 'analytics'
      }
      return route.name === 'Taskboard' ? 'taskboard' : 'kanban'
    }

    const viewMode = ref<'kanban' | 'taskboard' | 'analytics'>(getInitialViewMode())
    const taskboardData = ref<Taskboard | null>(null)
    const loadingTaskboard = ref(false)

    const toggleView = (mode: 'kanban' | 'taskboard' | 'analytics') => {
      viewMode.value = mode
      router.replace({ ...route, query: { ...route.query, view: mode } })
    }

    const fetchTaskboard = async () => {
      if (!props.selectedSprintId) return
      
      loadingTaskboard.value = true
      try {
        const response = await api.getSprintTaskboard(props.selectedSprintId)
        taskboardData.value = response.data
      } catch (error) {
        console.error('Failed to fetch taskboard:', error)
      } finally {
        loadingTaskboard.value = false
      }
    }

    watch([viewMode, () => props.selectedSprintId], () => {
      if (viewMode.value === 'taskboard' && props.selectedSprintId) {
        fetchTaskboard()
      }
    }, { immediate: true })

    watch(() => route.query.view, (newView) => {
      const mode = newView as string
      if (['kanban', 'taskboard', 'analytics'].includes(mode)) {
        viewMode.value = mode as 'kanban' | 'taskboard' | 'analytics'
      }
    })

    watch(() => route.name, (newName) => {
      // Prioritize query param, fallback to route name logic
      if (!route.query.view) {
        viewMode.value = newName === 'Taskboard' ? 'taskboard' : 'kanban'
      }
    })

    // Re-fetch when items might have changed
    watch(() => props.workItems, () => {
      if (viewMode.value === 'taskboard') {
        fetchTaskboard()
      }
    }, { deep: true })

    const handleUpdateTaskStatus = (task: TaskboardTask, newStatus: string) => {
      const fullTask = props.workItems.find(w => w.id === task.id)
      if (fullTask) {
        emit('update-status', fullTask, newStatus)
      }
    }

    const onAddTask = (parentId: number) => {
      const parent = props.workItems.find(w => w.id === parentId)
      if (parent) {
        emit('add-child-task', parent)
      } else if (parentId === 0) {
        // Handle unparented task creation - for now just open create dialog at 'To Do'
        emit('create-workitem', 'To Do')
      }
    }

    return {
      selectedSprint,
      filteredWorkItems,
      viewMode,
      taskboardData,
      loadingTaskboard,
      toggleView,
      handleUpdateTaskStatus,
      onAddTask
    }
  }
})
</script>