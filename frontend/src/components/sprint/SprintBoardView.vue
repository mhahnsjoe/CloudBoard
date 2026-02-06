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
      @work-item-updated="$emit('work-item-updated', $event)"
    />

    <!-- Taskboard View -->
    <SprintTaskboard
      v-else-if="viewMode === 'taskboard'"
      :taskboard="taskboardData"
      :loading="loadingTaskboard"
      :boardId="boardId"
      @view-details="$emit('view-details', $event)"
      @add-task="onAddTask"
      @update-task-status="handleUpdateTaskStatus"
      @work-item-updated="handleTaskboardItemUpdate"
    />

    <!-- Analytics View -->
    <SprintAnalytics 
      v-else-if="viewMode === 'analytics' && selectedSprint"
      :sprint="selectedSprint"
      :boardId="boardId"
      @update-sprint="$emit('update-sprint', $event)"
    />

    <!-- Quick Assign Modal -->

  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, watch, type PropType, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTeamsStore } from '@/stores/teams'
import type { Board } from '@/types/Project'
import type { WorkItem } from '@/types/WorkItem'
import type { Sprint } from '@/types/Sprint'
import type { Taskboard, TaskboardTask, TaskboardRow } from '@/types/Taskboard'
import * as api from '@/services/api'
import BoardHeader from '../board/BoardHeader.vue'
import SprintInfoBar from '../sprint/SprintInfoBar.vue'
import SprintSelector from '../sprint/SprintSelector.vue'
import BoardCanvas from '../board/BoardCanvas.vue'
import SprintTaskboard from './taskboard/SprintTaskboard.vue'
import SprintAnalytics from './SprintAnalytics.vue'
import QuickAssignModal from '@/components/workItem/QuickAssignModal.vue'

export default defineComponent({
  name: 'SprintBoardView',
  components: {
    BoardHeader,
    SprintInfoBar,
    SprintSelector,
    BoardCanvas,
    SprintTaskboard,
    SprintAnalytics,
    QuickAssignModal
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
    'update-sprint',
    'work-item-updated'
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



    
    
    const teamsStore = useTeamsStore()
    
    const toggleView = (mode: 'kanban' | 'taskboard' | 'analytics') => {
      viewMode.value = mode
      router.replace({ ...route, query: { ...route.query, view: mode } })
    }

    const generateTaskboard = () => {
      if (!props.selectedSprintId || !props.board) return
      
      const sprintItems = props.workItems
      const columns = (props.board!.columns || []).map(c => c.name)

      // Filter to PBI and standalone Bug as row headers
      // (PBI or Bug with NO parent)
      const parentItems = sprintItems.filter(w => 
        w.type === 'PBI' || (w.type === 'Bug' && !w.parentId)
      )

      const rows = parentItems.map(parent => {
         const childTasks = sprintItems.filter(w => 
            w.parentId === parent.id && (w.type === 'Task' || w.type === 'Bug')
         )

         // Calculate stats (replicating backend logic roughly)
         const totalHours = childTasks.reduce((sum, t) => sum + (t.estimatedHours || 0), 0) + (parent.estimatedHours || 0)
         const completedHours = childTasks.filter(t => t.status === 'Done').reduce((sum, t) => sum + (t.estimatedHours || 0), 0) 
                              + (parent.status === 'Done' ? (parent.estimatedHours || 0) : 0)
         
         // Note: TaskboardRow interface might need casting if WorkItem shapes align well enough
         // But we are constructing it explicitly
         return {
            ...parent, // Spread full WorkItem properties to satisfy interface
            id: parent.id,
            title: parent.title,
            type: parent.type as 'PBI' | 'Bug' | 'Unparented', // Type assertion
            status: parent.status,
            priority: parent.priority,
            totalHours,
            completedHours,
            remainingHours: childTasks.reduce((sum, t) => sum + (t.remainingHours || 0), 0) + (parent.remainingHours || 0),
            progressPercentage: totalHours > 0 ? (completedHours / totalHours) * 100 : 0,
            assignedToId: parent.assignedToId,
            assignedToName: parent.assignedToName,
            tasks: childTasks.map(t => ({
               id: t.id,
               title: t.title,
               type: t.type as 'Task' | 'Bug',
               status: t.status,
               priority: t.priority,
               estimatedHours: t.estimatedHours ?? null,
               actualHours: t.actualHours ?? null,
               remainingHours: t.remainingHours ?? null,
               parentId: parent.id,
               assignedToId: t.assignedToId ?? null,
               assignedToName: t.assignedToName ?? null
            }))
         } as unknown as TaskboardRow
      })

      // Handle orphans
      const orphanTasks = sprintItems.filter(w => 
         w.type === 'Task' && !w.parentId
      )

      if (orphanTasks.length > 0) {
         rows.push({
            id: 0,
            title: 'Unparented Tasks',
            type: 'Unparented', // NOTE: specialized type for UI
            status: 'N/A',
            priority: 'N/A',
            // Mock required properties for WorkItem interface compatibility
            createdAt: new Date().toISOString(),
            description: '',
            boardId: props.boardId,
            totalHours: orphanTasks.reduce((sum, t) => sum + (t.estimatedHours || 0), 0),
            completedHours: orphanTasks.filter(t => t.status === 'Done').reduce((sum, t) => sum + (t.estimatedHours || 0), 0),
            remainingHours: orphanTasks.reduce((sum, t) => sum + (t.remainingHours || 0), 0),
            progressPercentage: 0,
            tasks: orphanTasks.map(t => ({
               id: t.id,
               title: t.title,
               type: t.type as 'Task' | 'Bug',
               status: t.status,
               priority: t.priority,
               estimatedHours: t.estimatedHours ?? null,
               actualHours: t.actualHours ?? null,
               remainingHours: t.remainingHours ?? null,
               parentId: 0,
               assignedToId: t.assignedToId ?? null,
               assignedToName: t.assignedToName ?? null
            }))
         } as unknown as TaskboardRow)
      }

      taskboardData.value = {
         sprintId: props.selectedSprintId!,
         sprintName: props.sprints.find(s => s.id === props.selectedSprintId)?.name || '',
         columns: columns.length > 0 ? columns : ['To Do', 'In Progress', 'Done'],
         rows: rows,
         totalHours: 0, // Recalculate if needed for header, or sum rows
         completedHours: 0,
         totalTasks: 0,
         completedTasks: 0
      }
    }

    // Generate initially and when data changes
    watch([() => props.workItems, () => props.selectedSprintId], () => {
       generateTaskboard()
    }, { immediate: true, deep: true })
    
    // View mode switching now just ensures query param, no fetch needed
    watch(() => route.query.view, (newView) => {
      const mode = newView as string
      if (['kanban', 'taskboard', 'analytics'].includes(mode)) {
        viewMode.value = mode as 'kanban' | 'taskboard' | 'analytics'
      }
    })



    const handleUpdateTaskStatus = (task: TaskboardTask, newStatus: string) => {
      const fullTask = props.workItems.find(w => w.id === task.id)
      if (fullTask) {
        emit('update-status', fullTask, newStatus)
      }
    }

    const handleTaskboardItemUpdate = (updatedItem: any) => {
       // Since we generate taskboard from props.workItems, we just need to emit the update.
       // The parent (BoardDetailView) will update its workItems state, which flows down via props,
       // triggering generateTaskboard() automatically.
       // However, for pure optimistic feel, we might want to manually update the prop reference locally 
       // if waiting for parent roundtrip is too slow, but usually Vue is fast enough.
       // But wait, props are readonly. We cannot mutate props.workItems.
       // We emit the event. BoardDetailView handles it.
       // If we want INSTANT 'no-flicker' update, we rely on BoardDetailView updating its state immediately.
       // BoardDetailView: handleWorkItemUpdated -> workItems.value[index] = updatedItem
       // This trigger props update -> watch -> generateTaskboard.
       
       // Just merge and emit.
      const fullItem = props.workItems.find(w => w.id === updatedItem.id)
      if (fullItem) {
        const mergedItem = {
          ...fullItem,
          ...updatedItem
        }
        emit('work-item-updated', mergedItem)
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
      handleTaskboardItemUpdate,
      onAddTask,
    }
  }
})
</script>