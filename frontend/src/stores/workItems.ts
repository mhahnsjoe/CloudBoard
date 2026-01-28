import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as api from '@/services/api'
import type { WorkItem, WorkItemCreate, WorkItemEdit } from '@/types/WorkItem'

export const useWorkItemStore = defineStore('workItems', () => {
  // State
  const workItems = ref<WorkItem[]>([])
  const currentBoardId = ref<number | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const workItemsByStatus = computed(() => {
    const grouped: Record<string, WorkItem[]> = {}
    workItems.value.forEach(item => {
      if (!grouped[item.status]) {
        grouped[item.status] = []
      }
      grouped[item.status]!.push(item)
    })
    return grouped
  })

  const backlogItems = computed(() =>
    workItems.value.filter(item => !item.boardId)
  )

  const boardItems = computed(() =>
    workItems.value.filter(item => !!item.boardId)
  )

  // Actions
  async function fetchWorkItems(boardId: number) {
    loading.value = true
    error.value = null
    currentBoardId.value = boardId

    try {
      const response = await api.getWorkItems(boardId)
      workItems.value = response.data
      return workItems.value
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch work items'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchBacklogItems(projectId: number) {
    loading.value = true
    error.value = null

    try {
      const response = await api.getProjectBacklog(projectId)
      // Merge with existing items, replacing backlog items
      const boardItems = workItems.value.filter(item => !!item.boardId)
      workItems.value = [...boardItems, ...response.data]
      return response.data
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch backlog'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function createWorkItem(boardId: number, data: WorkItemCreate) {
    loading.value = true
    error.value = null

    try {
      const response = await api.createWorkItem(boardId, data)
      workItems.value.push(response.data)
      return response.data
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to create work item'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateWorkItem(boardId: number, id: number, data: WorkItemEdit) {
    loading.value = true
    error.value = null

    try {
      await api.updateWorkItem(boardId, id, data)

      // Update local state - preserve existing fields not in data
      const index = workItems.value.findIndex(w => w.id === id)
      if (index !== -1) {
        const existing = workItems.value[index]!
        workItems.value[index] = {
          id: existing.id,
          title: data.title,
          status: data.status,
          priority: data.priority,
          type: data.type as any,
          description: data.description,
          dueDate: data.dueDate,
          estimatedHours: data.estimatedHours,
          actualHours: data.actualHours,
          boardId: data.boardId,
          createdAt: existing.createdAt,
          parentId: data.parentId,
          sprintId: data.sprintId,
          parent: existing.parent,
          children: existing.children,
          level: existing.level,
          canHaveChildren: existing.canHaveChildren,
          hasChildren: existing.hasChildren,
          backlogOrder: existing.backlogOrder,
          totalEstimatedHours: existing.totalEstimatedHours,
          totalActualHours: existing.totalActualHours,
          completionPercentage: existing.completionPercentage
        }
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to update work item'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteWorkItem(boardId: number, id: number) {
    loading.value = true
    error.value = null

    try {
      await api.deleteWorkItem(boardId, id)
      workItems.value = workItems.value.filter(w => w.id !== id)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to delete work item'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateStatus(workItem: WorkItem, newStatus: string) {
    if (!workItem.boardId) return

    return updateWorkItem(workItem.boardId, workItem.id, {
      ...workItem,
      status: newStatus,
      type: workItem.type as string
    })
  }

  async function returnToBacklog(workItemId: number) {
    loading.value = true
    error.value = null

    try {
      await api.returnWorkItemToBacklog(workItemId)

      // Update local state - remove from current list or update if needed
      const index = workItems.value.findIndex(w => w.id === workItemId)
      if (index !== -1) {
        // Remove from list or update based on your needs
        workItems.value.splice(index, 1)
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to return to backlog'
      throw e
    } finally {
      loading.value = false
    }
  }

  function clearWorkItems() {
    workItems.value = []
    currentBoardId.value = null
  }

  return {
    // State
    workItems,
    currentBoardId,
    loading,
    error,
    // Getters
    workItemsByStatus,
    backlogItems,
    boardItems,
    // Actions
    fetchWorkItems,
    fetchBacklogItems,
    createWorkItem,
    updateWorkItem,
    deleteWorkItem,
    updateStatus,
    returnToBacklog,
    clearWorkItems
  }
})
