export interface Sprint {
  id: number
  name: string
  startDate: string
  endDate: string
  goal?: string
  status: SprintStatus
  createdAt: string
  boardId: number
  totalWorkItems: number
  completedWorkItems: number
  progressPercentage: number
  totalEstimatedHours: number
  completedEstimatedHours: number
  daysRemaining: number
}

export enum SprintStatus {
  Planning = 0,
  Active = 1,
  Completed = 2
}

export interface CreateSprintDto {
  name: string
  startDate: string
  endDate: string
  goal?: string
}

export interface UpdateSprintDto {
  name?: string
  startDate?: string
  endDate?: string
  goal?: string
}

export interface SprintStats {
  totalItems: number
  todoCount: number
  inProgressCount: number
  doneCount: number
  totalEstimatedHours: number
  completedEstimatedHours: number
  remainingEstimatedHours: number
}

export interface BurndownPoint {
  date: string
  remainingHours: number
  idealRemainingHours: number
}