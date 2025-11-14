import type { Sprint } from '@/types/Sprint'

/**
 * Utility functions for date formatting
 */

/**
 * Format a date to a localized string
 * @param date - Date string or Date object
 * @param options - Intl.DateTimeFormatOptions
 */
export function formatDate(
  date: string | Date,
  options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  }
): string {
  return new Date(date).toLocaleDateString('en-US', options)
}

/**
 * Format a date range for a sprint
 * @param sprint - Sprint object with startDate and endDate
 */
export function formatDateRange(sprint: Sprint): string {
  const start = new Date(sprint.startDate).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric'
  })
  const end = new Date(sprint.endDate).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
  return `${start} - ${end}`
}

/**
 * Format a date to just show month and day
 * @param date - Date string or Date object
 */
export function formatShortDate(date: string | Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric'
  })
}

/**
 * Format a date to show full date with time
 * @param date - Date string or Date object
 */
export function formatDateTime(date: string | Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * Calculate days between two dates
 * @param startDate - Start date
 * @param endDate - End date
 */
export function daysBetween(startDate: string | Date, endDate: string | Date): number {
  const start = new Date(startDate).getTime()
  const end = new Date(endDate).getTime()
  return Math.ceil((end - start) / (1000 * 60 * 60 * 24))
}

/**
 * Calculate days remaining until a date (negative if past)
 * @param targetDate - Target date
 */
export function daysRemaining(targetDate: string | Date): number {
  return daysBetween(new Date(), targetDate)
}
