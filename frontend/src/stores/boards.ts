import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as api from '@/services/api'
import type { Board, BoardCreate, BoardColumn } from '@/types/Project'

export const useBoardStore = defineStore('boards', () => {
  const boards = ref<Board[]>([])
  const currentProjectId = ref<number | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const boardsByType = computed(() => {
    return {
      kanban: boards.value.filter(b => b.type === 'Kanban'),
      scrum: boards.value.filter(b => b.type === 'Scrum'),
      backlog: boards.value.filter(b => b.type === 'Backlog')
    }
  })

  async function fetchBoards(projectId: number) {
    // Skip if already loaded for this project
    if (currentProjectId.value === projectId && boards.value.length > 0) {
      return boards.value
    }

    loading.value = true
    error.value = null
    currentProjectId.value = projectId

    try {
      const response = await api.getBoards(projectId)
      boards.value = response.data
      return boards.value
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to load boards'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function refreshBoards() {
    if (!currentProjectId.value) return
    
    loading.value = true
    error.value = null
    
    try {
      const response = await api.getBoards(currentProjectId.value)
      boards.value = response.data
      return boards.value
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to refresh boards'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function createBoard(projectId: number, boardData: BoardCreate) {
    loading.value = true
    error.value = null

    try {
      const response = await api.createBoard(projectId, boardData)
      const newBoard = response.data
      boards.value.push(newBoard)
      return newBoard
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to create board'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateBoard(projectId: number, boardId: number, boardData: BoardCreate) {
    loading.value = true
    error.value = null

    try {
      const response = await api.updateBoard(projectId, boardId, boardData)

      // Update local state with the response from API (includes proper column IDs)
      const index = boards.value.findIndex(b => b.id === boardId)
      if (index !== -1) {
        boards.value[index] = response.data
      }
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to update board'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteBoard(projectId: number, boardId: number) {
    loading.value = true
    error.value = null

    try {
      await api.deleteBoard(projectId, boardId)
      boards.value = boards.value.filter(b => b.id !== boardId)
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to delete board'
      error.value = message
      throw e
    } finally {
      loading.value = false
    }
  }

  function getBoardById(boardId: number) {
    return boards.value.find(b => b.id === boardId) || null
  }

  /**
   * Get ordered columns for a specific board
   * @param boardId - The board ID
   * @returns Sorted array of board columns
   */
  function getOrderedColumns(boardId: number): BoardColumn[] {
    const board = boards.value.find(b => b.id === boardId)
    if (!board?.columns) return []
    return [...board.columns].sort((a, b) => a.order - b.order)
  }

  /**
   * Get column names as array (for dropdowns, status selectors, etc.)
   * @param boardId - The board ID
   * @returns Array of column names in order
   */
  function getColumnNames(boardId: number): string[] {
    return getOrderedColumns(boardId).map(c => c.name)
  }

  function $reset() {
    boards.value = []
    currentProjectId.value = null
    loading.value = false
    error.value = null
  }

  return {
    boards,
    currentProjectId,
    loading,
    error,
    boardsByType,
    fetchBoards,
    refreshBoards,
    createBoard,
    updateBoard,
    deleteBoard,
    getBoardById,
    getOrderedColumns,
    getColumnNames,
    $reset
  }
})