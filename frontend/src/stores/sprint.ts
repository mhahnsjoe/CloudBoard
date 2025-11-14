import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as api from '@/services/api'
import type { Sprint, CreateSprintDto, UpdateSprintDto } from '@/types/Sprint'

export const useSprintStore = defineStore('sprint', () => {
  const sprints = ref<Sprint[]>([])
  const currentSprint = ref<Sprint | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const activeSprint = computed(() => 
    sprints.value.find(s => s.status === 'Active') 
  )

  const planningSprints = computed(() =>
    sprints.value.filter(s => s.status === 'Completed') 
  )

  const completedSprints = computed(() =>
    sprints.value.filter(s => s.status === 'Completed')
  )

  async function fetchSprints(boardId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await api.getSprints(boardId)
      sprints.value = response.data
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to load sprints'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchSprint(sprintId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await api.getSprint(sprintId)
      currentSprint.value = response.data
      return currentSprint.value
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to load sprint'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function createSprint(boardId: number, data: CreateSprintDto) {
    loading.value = true
    error.value = null
    try {
      const response = await api.createSprint(boardId, data)
      const newSprint = response.data
      sprints.value.unshift(newSprint)
      return newSprint
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to create sprint'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateSprint(sprintId: number, data: UpdateSprintDto) {
    loading.value = true
    error.value = null
    try {
      await api.updateSprint(sprintId, data)
      
      // Refetch the updated sprint to get complete data
      const response = await api.getSprint(sprintId)
      const updatedSprint = response.data
      
      const index = sprints.value.findIndex(s => s.id === sprintId)
      if (index !== -1) {
        sprints.value[index] = updatedSprint
      }
      if (currentSprint.value?.id === sprintId) {
        currentSprint.value = updatedSprint
      }
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to update sprint'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function startSprint(sprintId: number) {
    loading.value = true
    error.value = null
    try {
      await api.startSprint(sprintId)
      const sprint = sprints.value.find(s => s.id === sprintId)
      if (sprint) {
        sprint.status = 'Active'
      }
      if (currentSprint.value?.id === sprintId) {
        currentSprint.value.status = 'Active'
      }
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to start sprint'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function completeSprint(sprintId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await api.completeSprint(sprintId)
      const result = response.data
      const sprint = sprints.value.find(s => s.id === sprintId)
      if (sprint) {
        sprint.status = 'Completed' // SprintStatus.Completed
      }
      if (currentSprint.value?.id === sprintId) {
        currentSprint.value.status = 'Completed'
      }
      return result
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to complete sprint'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteSprint(sprintId: number) {
    loading.value = true
    error.value = null
    try {
      await api.deleteSprint(sprintId)
      sprints.value = sprints.value.filter(s => s.id !== sprintId)
      if (currentSprint.value?.id === sprintId) {
        currentSprint.value = null
      }
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to delete sprint'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function assignWorkItemToSprint(workItemId: number, sprintId: number | null) {
    loading.value = true
    error.value = null
    try {
      await api.assignWorkItemToSprint(workItemId, sprintId)
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to assign work item'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  function $reset() {
    sprints.value = []
    currentSprint.value = null
    loading.value = false
    error.value = null
  }

  return {
    sprints,
    currentSprint,
    loading,
    error,
    activeSprint,
    planningSprints,
    completedSprints,
    fetchSprints,
    fetchSprint,
    createSprint,
    updateSprint,
    startSprint,
    completeSprint,
    deleteSprint,
    assignWorkItemToSprint,
    $reset
  }
})