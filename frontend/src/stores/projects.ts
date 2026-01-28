import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as api from '@/services/api'
import type { Project, ProjectCreate } from '@/types/Project'

export const useProjectStore = defineStore('projects', () => {
  // State
  const projects = ref<Project[]>([])
  const currentProject = ref<Project | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const projectCount = computed(() => projects.value.length)

  const getProjectById = computed(() => {
    return (id: number) => projects.value.find(p => p.id === id)
  })

  const sortedProjects = computed(() => {
    return [...projects.value].sort((a, b) => b.id - a.id)
  })

  // Actions
  async function fetchProjects() {
    loading.value = true
    error.value = null

    try {
      const response = await api.getProjects()
      projects.value = response.data
      return projects.value
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch projects'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchProject(id: number) {
    loading.value = true
    error.value = null

    try {
      const response = await api.getProject(id)
      currentProject.value = response.data

      // Update in list if exists
      const index = projects.value.findIndex(p => p.id === id)
      if (index !== -1) {
        projects.value[index] = response.data
      }

      return response.data
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch project'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function createProject(data: ProjectCreate) {
    loading.value = true
    error.value = null

    try {
      const response = await api.createProject(data)
      projects.value.push(response.data)
      return response.data
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to create project'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateProject(id: number, data: ProjectCreate) {
    loading.value = true
    error.value = null

    try {
      await api.updateProject(id, data)

      // Update local state
      const index = projects.value.findIndex(p => p.id === id)
      if (index !== -1) {
        const existing = projects.value[index]!
        projects.value[index] = {
          id: existing.id,
          name: data.name,
          description: data.description,
          boards: existing.boards
        }
      }

      if (currentProject.value?.id === id) {
        currentProject.value = {
          id: currentProject.value.id,
          name: data.name,
          description: data.description,
          boards: currentProject.value.boards
        }
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to update project'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteProject(id: number) {
    loading.value = true
    error.value = null

    try {
      await api.deleteProject(id)

      // Remove from local state
      projects.value = projects.value.filter(p => p.id !== id)

      if (currentProject.value?.id === id) {
        currentProject.value = null
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to delete project'
      throw e
    } finally {
      loading.value = false
    }
  }

  function setCurrentProject(project: Project | null) {
    currentProject.value = project
  }

  function clearError() {
    error.value = null
  }

  return {
    // State
    projects,
    currentProject,
    loading,
    error,
    // Getters
    projectCount,
    getProjectById,
    sortedProjects,
    // Actions
    fetchProjects,
    fetchProject,
    createProject,
    updateProject,
    deleteProject,
    setCurrentProject,
    clearError
  }
})
