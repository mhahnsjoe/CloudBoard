/**
 * Utility functions for badge styling
 */

import type { BoardColumn } from '@/types/Project'

/**
 * Get badge class based on workflow category
 */
export function getStatusBadgeClassByCategory(category: string): string {
  const classes: Record<string, string> = {
    'To Do': 'bg-gray-100 text-gray-700',
    'In Progress': 'bg-blue-100 text-blue-700',
    'Done': 'bg-green-100 text-green-700'
  }
  return classes[category] || 'bg-gray-100 text-gray-700'
}

/**
 * Get badge class for a status name
 * If columns are provided, uses category-based styling
 * Otherwise falls back to legacy name-based styling
 */
export function getStatusBadgeClass(status: string, columns?: BoardColumn[]): string {
  if (columns) {
    const column = columns.find(c => c.name === status)
    if (column) return getStatusBadgeClassByCategory(column.category)
  }

  // Fallback to legacy behavior for backward compatibility
  const legacyClasses: Record<string, string> = {
    'To Do': 'bg-gray-100 text-gray-700',
    'In Progress': 'bg-blue-100 text-blue-700',
    'Done': 'bg-green-100 text-green-700'
  }
  return legacyClasses[status] || 'bg-gray-100 text-gray-700'
}

export function getPriorityBadgeClass(priority: string): string {
  const classes: Record<string, string> = {
    'Low': 'bg-gray-100 text-gray-600',
    'Medium': 'bg-yellow-100 text-yellow-700',
    'High': 'bg-orange-100 text-orange-700',
    'Critical': 'bg-red-100 text-red-700'
  }
  return classes[priority] || 'bg-gray-100 text-gray-600'
}

export function getBoardTypeClass(type: string): string {
  const classes: Record<string, string> = {
    'Kanban': 'bg-blue-100 text-blue-700',
    'Scrum': 'bg-green-100 text-green-700',
    'Backlog': 'bg-purple-100 text-purple-700'
  }
  return classes[type] || 'bg-gray-100 text-gray-700'
}

export function getSprintStatusClass(status: string): string {
  const classes: Record<string, string> = {
    'Planning': 'bg-yellow-100 text-yellow-800',
    'Active': 'bg-green-100 text-green-800',
    'Completed': 'bg-gray-100 text-gray-800'
  }
  return classes[status] || 'bg-gray-100 text-gray-800'
}

/**
 * Get icon class based on workflow category
 */
export function getStatusIconClassByCategory(category: string): string {
  const classes: Record<string, string> = {
    'To Do': 'text-gray-500',
    'In Progress': 'text-blue-500',
    'Done': 'text-green-500'
  }
  return classes[category] || 'text-gray-500'
}

/**
 * Get icon class for a status name
 * If columns are provided, uses category-based styling
 * Otherwise falls back to legacy name-based styling
 */
export function getStatusIconClass(status: string, columns?: BoardColumn[]): string {
  if (columns) {
    const column = columns.find(c => c.name === status)
    if (column) return getStatusIconClassByCategory(column.category)
  }

  // Fallback to legacy behavior for backward compatibility
  const legacyClasses: Record<string, string> = {
    'To Do': 'text-gray-500',
    'In Progress': 'text-blue-500',
    'Done': 'text-green-500'
  }
  return legacyClasses[status] || 'text-gray-500'
}