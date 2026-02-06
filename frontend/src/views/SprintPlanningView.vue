<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <!-- Header -->
    <div class="mb-6">
      <nav class="text-sm text-gray-500 mb-2">
        <router-link :to="`/projects/${projectId}/boards/${boardId}`" class="hover:text-blue-600">
          {{ projectName }}
        </router-link>
        <span class="mx-2">›</span>
        <span>Sprint Planning</span>
      </nav>
      <h1 class="text-3xl font-bold text-gray-900">Sprint Planning</h1>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon class="h-5 w-5 text-blue-600" />
      Loading...
    </div>

    <!-- Planning Board -->
    <div v-else class="h-[calc(100vh-200px)]">
      <SprintPlanningBoard
        :planningContext="planningContext"
        :selectedSprintId="selectedSprintId"
        :sprintCapacity="sprintCapacity"
        :sprintItems="sprintItems"
        @select-sprint="handleSelectSprint"
        @move-to-sprint="handleMoveToSprint"
        @move-to-backlog="handleMoveToBacklog"
        @create-sprint="openCreateSprintModal"
        @start-sprint="handleStartSprint"
        @edit-capacity="openCapacityModal"
      />
    </div>

    <!-- Sprint Modal -->
    <SprintModal
      v-if="showSprintModal"
      :sprint="editingSprint || undefined"
      :boardId="boardId"
      @close="showSprintModal = false"
      @save="handleSaveSprint"
    />

    <!-- Capacity Modal -->
    <SprintCapacityModal
      v-if="showCapacityModal"
      :sprint="selectedSprint"
      :currentCapacity="sprintCapacity?.totalCapacityHours || 0"
      @close="showCapacityModal = false"
      @save="handleSaveCapacity"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { 
  getSprintPlanningContext, 
  bulkAssignToSprint, 
  bulkUnassignFromSprint,
  getSprintCapacity,
  setSprintCapacity,
  startSprint,
  createSprint,
  getBoard,
  getWorkItems
} from '@/services/api'
import type { 
  SprintPlanningContext, 
  SprintCapacity, 
  WorkItemSummary,
  Sprint,
  CreateSprintDto 
} from '@/types/Sprint'
import type { WorkItem } from '@/types/WorkItem'
import SprintPlanningBoard from '@/components/sprint/SprintPlanningBoard.vue'
import SprintModal from '@/components/sprint/SprintModal.vue'
import SprintCapacityModal from '@/components/sprint/SprintCapacityModal.vue'
import { LoadingIcon } from '@/components/icons'
import { useToast } from '@/composables/useToast'

const route = useRoute()
const { error: toastError } = useToast()
const projectId = computed(() => Number(route.params.projectId))
const boardId = computed(() => Number(route.params.boardId))

// State
const loading = ref(true)
const projectName = ref('Project')
const planningContext = ref<SprintPlanningContext | null>(null)
const selectedSprintId = ref<number | null>(null)
const sprintCapacity = ref<SprintCapacity | null>(null)
const sprintItems = ref<WorkItemSummary[]>([])
const showSprintModal = ref(false)
const showCapacityModal = ref(false)
const editingSprint = ref<Sprint | null>(null)

// Computed
const selectedSprint = computed(() => 
  planningContext.value?.sprints.find(s => s.id === selectedSprintId.value)
)

// Methods
const fetchPlanningContext = async () => {
  loading.value = true
  try {
    const [contextRes, boardRes] = await Promise.all([
      getSprintPlanningContext(boardId.value),
      getBoard(projectId.value, boardId.value)
    ])
    planningContext.value = contextRes.data
    projectName.value = boardRes.data.name || 'Project'
    
    const context = planningContext.value
    if (context) {
      // Check URL query param first
      if (route.query.sprintId) {
        const queryId = Number(route.query.sprintId)
        const exists = context.sprints.find(s => s.id === queryId)
        if (exists) {
          selectedSprintId.value = queryId
        }
      }

      // Auto-select active sprint if exists and none selected
      if (!selectedSprintId.value) {
        const activeSprint = context.sprints.find(s => s.status === 'Active')
        if (activeSprint) {
          selectedSprintId.value = activeSprint.id
        } else {
          // Prefer Planning sprints over Completed ones
          const planSprint = context.sprints.find(s => s.status === 'Planning')
          if (planSprint) {
            selectedSprintId.value = planSprint.id
          } else if (context.sprints.length > 0 && context.sprints[0]) {
            selectedSprintId.value = context.sprints[0].id
          }
        }
      }
    }
  } catch (error) {
    console.error('Failed to fetch planning context:', error)
  } finally {
    loading.value = false
  }
}

const fetchSprintData = async () => {
  if (!selectedSprintId.value) {
    sprintCapacity.value = null
    sprintItems.value = []
    return
  }
  
  try {
    const [capacityRes, itemsRes] = await Promise.all([
      getSprintCapacity(selectedSprintId.value),
      getWorkItems(boardId.value) // We'll filter these below
    ])
    
    sprintCapacity.value = capacityRes.data
    
    // Filter items belonging to selected sprint
    const allSprintItems = (itemsRes.data as WorkItem[])
      .filter(w => w.sprintId === selectedSprintId.value)
    
    // Group hierarchically - only show root items with children
    const rootItems = allSprintItems.filter(w => !w.parentId)
    
    sprintItems.value = rootItems.map(w => {
      const children = allSprintItems.filter(c => c.parentId === w.id)
      const totalHours = (w.estimatedHours || 0) + children.reduce((sum, c) => sum + (c.estimatedHours || 0), 0)
      
      return {
        id: w.id,
        title: w.title,
        type: w.type,
        status: w.status,
        priority: w.priority,
        estimatedHours: totalHours,
        parentId: w.parentId ?? null,
        parentTitle: w.parent?.title ?? null,
        childCount: children.length,
        children: children.map(c => ({
          id: c.id,
          title: c.title,
          type: c.type,
          status: c.status,
          priority: c.priority,
          estimatedHours: c.estimatedHours ?? 0,
          parentId: c.parentId ?? null,
          parentTitle: w.title ?? null,
          childCount: 0,
          children: [] as WorkItemSummary[]
        }))
      }
    }) as WorkItemSummary[]
  } catch (error) {
    console.error('Failed to fetch sprint data:', error)
  }
}

const handleSelectSprint = (sprintId: number | null) => {
  selectedSprintId.value = sprintId
}

const handleMoveToSprint = async (itemIds: number[]) => {
  if (!selectedSprintId.value) return
  try {
    const res = await bulkAssignToSprint(selectedSprintId.value, itemIds)
    if (res.data.failedCount > 0) {
      toastError(`Failed to assign items:\n${res.data.errors.join('\n')}`)
    }
    await Promise.all([fetchPlanningContext(), fetchSprintData()])
  } catch (error) {
    console.error('Failed to assign items:', error)
    toastError('Failed to assign items to sprint')
  }
}

const handleMoveToBacklog = async (itemIds: number[]) => {
  if (!selectedSprintId.value) return
  try {
    await bulkUnassignFromSprint(selectedSprintId.value, itemIds)
    await Promise.all([fetchPlanningContext(), fetchSprintData()])
  } catch (error) {
    console.error('Failed to unassign items:', error)
  }
}

const openCreateSprintModal = () => {
  editingSprint.value = null
  showSprintModal.value = true
}

const handleSaveSprint = async (sprintData: CreateSprintDto) => {
  try {
    await createSprint(boardId.value, sprintData)
    showSprintModal.value = false
    await fetchPlanningContext()
  } catch (error) {
    console.error('Failed to create sprint:', error)
  }
}

const handleStartSprint = async (sprintId: number) => {
  try {
    await startSprint(sprintId)
    await fetchPlanningContext()
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
  } catch (error: any) {
    console.error('Failed to start sprint:', error)
    toastError(error.response?.data || 'Failed to start sprint. Another sprint might be active.')
  }
}

const openCapacityModal = () => {
  showCapacityModal.value = true
}

const handleSaveCapacity = async (capacityHours: number) => {
  if (!selectedSprintId.value) return
  try {
    await setSprintCapacity(selectedSprintId.value, capacityHours)
    showCapacityModal.value = false
    await fetchSprintData()
  } catch (error) {
    console.error('Failed to set capacity:', error)
  }
}

// Watchers
watch(selectedSprintId, () => {
  fetchSprintData()
})

// Lifecycle
onMounted(() => {
  fetchPlanningContext()
})
</script>
