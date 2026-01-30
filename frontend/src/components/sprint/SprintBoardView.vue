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
         <div v-if="selectedSprintId" class="flex items-center gap-4">
            <!-- View Toggle -->
            <div class="flex items-center bg-gray-100 rounded-lg p-1 border border-gray-200">
              <button
                @click="toggleView('kanban')"
                class="px-3 py-1 text-xs font-medium rounded-md transition-all"
                :class="viewMode === 'kanban' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-600 hover:text-gray-900'"
              >
                Board
              </button>
              <button
                @click="toggleView('taskboard')"
                class="px-3 py-1 text-xs font-medium rounded-md transition-all"
                :class="viewMode === 'taskboard' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-600 hover:text-gray-900'"
              >
                Taskboard
              </button>
            </div>

            <nav class="flex items-center gap-2">
              <router-link
                v-if="selectedSprint?.status === 'Planning'"
                :to="`/projects/${board?.projectId}/boards/${boardId}/sprint-planning?sprintId=${selectedSprintId}`"
                class="text-xs font-medium text-gray-600 hover:text-blue-600 px-2 py-1 rounded hover:bg-gray-100 transition-all"
                active-class="text-blue-600 bg-blue-50"
              >
                Planning
              </router-link>
              <router-link
                v-if="selectedSprintId"
                :to="`/projects/${board?.projectId}/boards/${boardId}/sprints/${selectedSprintId}/summary`"
                class="text-xs font-medium text-gray-600 hover:text-blue-600 px-2 py-1 rounded hover:bg-gray-100 transition-all"
                active-class="text-blue-600 bg-blue-50"
              >
                Insights
              </router-link>
            </nav>
        </div>
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
      @create-workitem="$emit('create-workitem', $event)"
      @edit-workitem="$emit('edit-workitem', $event)"
      @delete-workitem="$emit('delete-workitem', $event)"
      @update-status="(item, status) => $emit('update-status', item, status)"
      @return-to-backlog="$emit('return-to-backlog', $event)"
      @add-child-task="$emit('add-child-task', $event)"
    />

    <!-- Taskboard View -->
    <SprintTaskboard
      v-else-if="viewMode === 'taskboard'"
      :taskboard="taskboardData"
      :loading="loadingTaskboard"
      @edit-workitem="$emit('edit-workitem', $event)"
      @add-task="onAddTask"
      @edit-task="$emit('edit-workitem', $event)"
      @update-task-status="handleUpdateTaskStatus"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, watch, onMounted, type PropType } from 'vue'
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

export default defineComponent({
  name: 'SprintBoardView',
  components: {
    BoardHeader,
    SprintInfoBar,
    SprintSelector,
    BoardCanvas,
    SprintTaskboard
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
    'add-child-task'
  ],
  setup(props, { emit }) {
    const route = useRoute()
    const router = useRouter()

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

    const viewMode = ref<'kanban' | 'taskboard'>(route.name === 'Taskboard' ? 'taskboard' : 'kanban')
    const taskboardData = ref<Taskboard | null>(null)
    const loadingTaskboard = ref(false)

    const toggleView = (mode: 'kanban' | 'taskboard') => {
      viewMode.value = mode
      const path = mode === 'taskboard' 
        ? `/projects/${props.board?.projectId}/boards/${props.boardId}/taskboard`
        : `/projects/${props.board?.projectId}/boards/${props.boardId}`
      
      router.push(path)
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

    watch(() => route.name, (newName) => {
      viewMode.value = newName === 'Taskboard' ? 'taskboard' : 'kanban'
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