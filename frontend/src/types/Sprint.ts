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
  capacityHours: number | null
  capacityUtilization: number
  retrospective: string | null
  retrospectiveDate: string | null
}

export type SprintStatus = 'Planning' | 'Active' | 'Completed'

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

export interface SprintPlanningContext {
  sprints: Sprint[]
  backlogItems: WorkItemSummary[]
  totalBacklogHours: number
  totalBacklogItems: number
}

export interface WorkItemSummary {
  id: number
  title: string
  type: string
  status: string
  priority: string
  estimatedHours: number | null
  parentId: number | null
  parentTitle: string | null
  childCount: number
  children: WorkItemSummary[]
}

export interface BulkOperationResult {
  successCount: number
  failedCount: number
  errors: string[]
}

export interface SprintCapacity {
  sprintId: number
  totalCapacityHours: number
  allocatedHours: number
  remainingHours: number
  utilizationPercentage: number
}

export interface SprintVelocity {
  sprintId: number
  sprintName: string
  startDate: string
  endDate: string
  plannedHours: number
  completedHours: number
  plannedItems: number
  completedItems: number
  velocityPercentage: number
}

export interface BoardVelocity {
  boardId: number
  sprintVelocities: SprintVelocity[]
  averageVelocityHours: number
  averageVelocityItems: number
  totalSprintsAnalyzed: number
}